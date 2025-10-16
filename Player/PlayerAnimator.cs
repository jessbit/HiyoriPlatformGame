using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[AddComponentMenu("Game/Player/Player Animator")]
public class PlayerAnimator : MonoBehaviour
{
    //定义一个强制过渡的类，用于指定从某个玩家状态退出时强制跳转到Animator中的某个动画
    [System.Serializable]
    public class ForcedTransition
    {
        [Tooltip("玩家状态机中'fromStateId状态结束时，强制跳转到某个动画")]
        public int fromStateId;

        [Tooltip("目标动画所在的Animator层索引，默认0表示Base Layer")]
        public int animatorLayer;

        [Tooltip("要强制播放的动画状态名")] 
        public string toAnimationState;
    }

    [Header("Parameters Names")]
    public string stateName = "State";
    public string lastStateName = "Last State";
    public string lateralSpeedName = "Lateral Speed";
    public string verticalSpeedName ="Vertical Speed";
    public string lateralAnimationSpeedName ="Lateral Animation Speed";
    public string healthName = "Health";
    public string jumpCounterName ="Jump Counter";
    public string isGroundedName ="Is Grounded";
    public string isHoldingName ="Is Holding";
    public string onStateChangedName ="On State Change";
    [Header("Settings")]
    public float minLateralAnimationSpeed = 0.5f;//横向速度最小播放速度，防止太慢

    public List<ForcedTransition> forcedTransitions; //强制转换列表
    public Animator animator;

    //Animator 参数的Hash值,避免查找开销
    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;
    
    protected Dictionary<int, ForcedTransition> m_forcedTransitions;
    protected Player m_player;
    protected virtual void Start()
    {
        InitializePlayer();
        InitializeForcedTransition();
        InitializeParameterHash();
        InitializeAnimatorTriggers();
    }

    protected virtual void LateUpdate() => HandleAnimatorParameters();
    
    protected virtual void InitializePlayer()
    {
        m_player=GetComponent<Player>();
        m_player.states.events.onChange.AddListener(HandleForcedTransition);
    }

    protected virtual void InitializeForcedTransition()
    {
        m_forcedTransitions = new Dictionary<int, ForcedTransition>();
        foreach (var transition in forcedTransitions)
        {
            if (!m_forcedTransitions.ContainsKey(transition.fromStateId))
            {
                m_forcedTransitions.Add(transition.fromStateId,transition);
            }
        }
    }
    protected virtual void HandleForcedTransition()
    {
        var lastStateIndex = m_player.states.lastIndex;
        if (m_forcedTransitions.ContainsKey(lastStateIndex))
        {
            var layer = m_forcedTransitions[lastStateIndex].animatorLayer;
            animator.Play(m_forcedTransitions[lastStateIndex].toAnimationState,layer);
        }
    }

    /// <summary>
    /// 参数名转换为Hash，提高性能
    /// </summary>
    protected virtual void InitializeParameterHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
        
    }

    /// <summary>
    /// 初始化Animator触发器，当状态切换时触发动画事件
    /// </summary>
    protected virtual void InitializeAnimatorTriggers()
    {
        m_player.states.events.onChange.AddListener(()=>animator.SetTrigger(m_onStateChangedHash));
    }

    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude;
        var verticalSpeed = m_player.verticalVelocity.y;
        //横向动画播放速度=》百分比 =横向速度/最大速度，保证最小速度不低于min
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);
        
        //设置Animator参数
        //根据速度驱动动画
        animator.SetInteger(m_stateHash,m_player.states.index);
        animator.SetInteger(m_lastStateHash,m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash,lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash,verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash,lateralAnimationSpeed);
        animator.SetInteger(m_jumpCounterHash,m_player.jumpCounter);
        animator.SetBool(m_isGroundedHash,m_player.isGrounded);
        animator.SetBool(m_isHoldingHash,m_player.holding);
    }
    
}
