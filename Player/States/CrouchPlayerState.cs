using UnityEngine;

public class CrouchPlayerState :PlayerState
{
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originalHeight);
    }

    protected override void OnStep(Player player)
    {
        player.Gravity();
        player.SnapToGround();//贴地
        player.Fall();
        player.Decelerate(player.stats.current.crouchFriction);
        //获取输入移动方向(相对世界不考虑相机)
        var inputDirection = player.inputs.GetMovementDirection();
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            if (inputDirection.sqrMagnitude > 0 && !player.holding)
            {
                //速度为0进入爬行状态
                if (player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<CrawlingPlayerState>();
                }
                //下蹲状态按下跳跃键为后空翻
                else if (player.inputs.GetJumpDown())
                {
                    player.Backflip(player.stats.current.backflipBackwardForce);
                }
            }
        }
        else
        {
            player.states.Change<IdlePlayerState>();

        }
    }

    public override void OnContact(Player player, Collider other)
    {
    }        
}