using System.Collections;
using UnityEngine;

public class LedgeHangingPlayerState:PlayerState
{
    //防止爬升时重置父对象
    protected bool m_keepParent;
    protected Coroutine m_cleanParentRoutine;
    protected const float k_clearParentDelay = 0.25f;
    protected override void OnEnter(Player player)
    {
        if (m_cleanParentRoutine != null)
            player.StopCoroutine(m_cleanParentRoutine);
        m_keepParent = false;
        player.skin.position += player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
        player.ResetJumps();
        player.ResetAirSpins();
        player.ResetAirDash();
    }

    protected override void OnExit(Player player)
    {
        m_cleanParentRoutine = player.StartCoroutine(CleanParentRoutine(player));
        player.skin.position -= player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
    }
    protected override void OnStep(Player player)
    {
        //顶端（平台面)检测参数
        //以玩家半径+可向前探测的最大距离作为顶端检测的向前探测范围
        var ledgeTopMaxDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;
        //顶端检测的数值
        //从较高处向下打射线扫到ledge的台面
        var ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardOffset;
        var topOrigin = player.position
                        + Vector3.up * ledgeTopHeightOffset
                        + player.transform.forward * ledgeTopMaxDistance;
        //侧面检测
        //起点:中心高度再向下一个可配置的偏移量
        //目的:再胸口/腹部附近用球体扫描前方墙体，获得贴墙的命中点和法线向量
        var sideOrigin = player.position + Vector3.up * player.height * 0.5f +
                         Vector3.down * player.stats.current.ledgeSideHeightOffset;
        var rayDistance = player.radius + player.stats.current.ledgeSideMaxDistance;
        var rayRadius = player.stats.current.ledgeSideCollisionRadius;  
        //侧面+顶端双重判定
        //从胸口用球形检测确定墙面，从头顶前方向下确定一个可以扒住的台面
        if (Physics.SphereCast(
                sideOrigin,//胸腹高度附近
                rayRadius,
                player.transform.forward,
                out var sideHit,
                rayDistance,
                player.stats.current.ledgeHangingLayers,
                QueryTriggerInteraction.Ignore)&&
            Physics.Raycast(
                topOrigin, //平台边缘上方
                Vector3.down,
                out var topHit,
                player.height,//足够覆盖玩家身高
                player.stats.current.ledgeHangingLayers,
                QueryTriggerInteraction.Ignore)
            )
        {
            //x左右，z前后
            var inputDirection = player.inputs.GetMovementDirection();
            //左右移动检测的起点
            var ledgeSideOrigin = sideOrigin + player.transform.right * Mathf.Sign(inputDirection.x) * player.radius;
            //从平台点向下一个半身位，刚好让角色胸部贴在平台正下方
            var ledgeHeight = topHit.point.y - player.height * 0.5f;
            //面向墙面
            var sideForward = -new Vector3(sideHit.normal.x, 0, sideHit.normal.z).normalized;
            //计算翻越的落点
            //平台命中点+上抬容纳胶囊体的高度+向前挪一个半径(避免卡边)
            var destinationHeight = player.height * 0.5f + Physics.defaultContactOffset;
            var climbDestination =
                topHit.point + Vector3.up * destinationHeight + player.transform.forward * player.radius;
            player.FaceDirection(sideForward);
            //左右移动
            if (Physics.Raycast(
                    ledgeSideOrigin,
                    sideForward,
                    rayDistance,
                    player.stats.current.ledgeHangingLayers,
                    QueryTriggerInteraction.Ignore))
            {
                player.lateralVelocity =
                    player.transform.right * inputDirection.x * player.stats.current.ledgeMovementSpeed;
            }
            else
            {
                player.lateralVelocity = Vector3.zero;
            }
            //约束玩家悬挂的位置
            //锁定在:命中点的水平位置+目标悬挂高度
            player.transform.position =
                new Vector3(sideHit.point.x,ledgeHeight,sideHit.point.z)
                -sideForward*player.radius
                -player.center;

            if (player.inputs.GetReleaseLedgeDown())
            {
                player.FaceDirection(-sideForward);
                player.states.Change<FallPlayerState>();
            }
            else if (player.inputs.GetJumpDown())
            {
                player.Jump(player.stats.current.maxJumpHeight);
            }
            //翻越
            //玩家有向前输入，允许翻越，平台所属层在可翻越层中
            //climbDestination能容纳玩家体积(不穿墙/不顶棚)
            else if (inputDirection.z > 0
                     && player.stats.current.canClimbLedges
                     && (((1 << topHit.collider.gameObject.layer) &
                          player.stats.current.ledgeClimbLayers) != 0)
                     && player.FitsIntoPosition(climbDestination))
            {
                //标记，切换状态期间保持父子关系(若动画/根节点需要)
                m_keepParent = true;
                player.states.Change<LedgeClimbingPlayerState>();
                player.playerEvents.OnLedgeClimbing?.Invoke();
            }
        }
        else
        {
            player.states.Change<FallPlayerState>();
        }
    }
    /// <summary>
    /// 延迟清理父对象协程
    /// 如果爬升保持时则跳过
    /// </summary>
    /// <param name="player"></param>
    protected virtual IEnumerator CleanParentRoutine(Player player)
    {
        if(m_keepParent) yield break;
        yield return new WaitForSeconds(k_clearParentDelay);
        player.transform.parent = null;
    }
    
    public override void OnContact(Player player, Collider other)
    {	
    }    
}