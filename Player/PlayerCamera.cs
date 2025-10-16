using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
[AddComponentMenu("Game/Player/Player Camera")]
public class PlayerCamera :MonoBehaviour
{
    [Header(("Camera Settings"))]
    public Player player;

    public float maxDistance = 15f;
    public float initialAngle = 20f; //初始俯仰角
    public float heightOffset = 1f; //垂直偏移量

    [Header("Following Settings")]
    public float verticalUpDeadZone = 0.15f;
    public float verticalDownDeadZone = 0.15f;
    public float verticalAirUpDeadZone = 4f;
    public float verticalAirDownDeadZone = 0f;
    public float maxVerticalSpeed = 10f;
    public float maxAirVerticalSpeed = 100f;
    [Header("Orbit Settings")]
    public bool canOrbit = true;//是否手动旋转
    public bool canOrbitWithVelocity = true; //是否允许通过速度带动摄像机
    public float orbitVelocityMultiplier = 5;//速度驱动相机旋转的倍率
    [Range(0, 90)] public float verticalMaxRotation = 80;
    [Range(-90, 0)] public float verticalMinRotation = -20;
    protected CinemachineVirtualCamera m_camera;
    protected CinemachineBrain m_brain;
    protected Cinemachine3rdPersonFollow m_cameraBody; //3D跟随组件

    protected float m_cameraDistance;
    protected float m_cameraTargetYaw;
    protected float m_cameraTargetPitch;
    protected Vector3 m_cameraTargetPosition;
    
    protected Transform m_target; //相机跟随的目标点(玩家上方一点)
    protected string k_targetName = "Camera Player Follower Target";
    protected virtual void Start()
    {
        InitializeComponents();
        InitializeFollower();
        InitializeCamera();
    }

    protected virtual void InitializeComponents()
    {
        if (!player)
        {
            player=FindObjectOfType<Player>();
        }
        m_camera =GetComponent<CinemachineVirtualCamera>();
        m_cameraBody = m_camera.AddCinemachineComponent<Cinemachine3rdPersonFollow>();
        m_brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    protected virtual void InitializeFollower()
    {
        //生成一个独立的目标节点
        m_target = new GameObject(k_targetName).transform;
        m_target.position=player.transform.position;
    }

    protected virtual void InitializeCamera()
    {
        m_camera.Follow = m_target.transform;
        m_camera.LookAt = player.transform;

        Reset();
    }

    /// <summary>
    /// 重置相机到初始位置和角度
    /// </summary>
    public virtual void Reset()
    {
        m_cameraDistance = maxDistance;
        m_cameraTargetPitch = initialAngle;//设定初始俯仰角
        m_cameraTargetYaw=player.transform.rotation.eulerAngles.y;//根据玩家朝向设定相机水平角
        m_cameraTargetPosition = player.unsizedPosition + Vector3.up * heightOffset; //不涉及碰撞
        MoveTarget();
        m_brain.ManualUpdate();//强制刷新相机
    }

    protected virtual void MoveTarget()
    {
        m_target.position = m_cameraTargetPosition;
        m_target.rotation = Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0.0f);
        m_cameraBody.CameraDistance =m_cameraDistance;
    }
    /// <summary>
    /// 判断是否处于需要竖直跟随的状态
    /// </summary>
    /// <returns></returns>
    protected virtual bool VerticalFollowingStates()
    {
        return player.states.IsCurrentOfType(typeof(SwimPlayerState)) ||
               player.states.IsCurrentOfType(typeof(PoleClimbingPlayerState)) ||
               player.states.IsCurrentOfType(typeof(WallDragPlayerState)) ||
               player.states.IsCurrentOfType(typeof(LedgeHangingPlayerState)) ||
               player.states.IsCurrentOfType(typeof(LedgeClimbingPlayerState)) ||
               player.states.IsCurrentOfType(typeof(RailGrindPlayerState));
    }
    protected virtual void HandleOrbit()
    {
        //判断是否允许手动环绕相机
        if (canOrbit)
        {
            //从玩家输入中获取视角方向
            //x->左右 (控制Yaw,水平旋转)
            //z->上下(控制Pitch,垂直旋转)
            
            var direction = player.inputs.GetLookDirection();
            if (direction.sqrMagnitude > 0)
            {
                var usingMouse = player.inputs.IsLookingWithMouse();
                //使用鼠标时输入是即时数值,每帧数返回对应帧输入值,不需要×deltaTime平滑处理，否则会受帧数影响导致帧越高转的越慢
                //使用手柄时输入是按帧累计，只输入方向速度，按帧数计算输入，需要×deltaTime保持平滑，是基于时间的连续旋转
                float deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;
                
                //修正旋转角
                m_cameraTargetYaw +=direction.x *deltaTimeMultiplier;
                m_cameraTargetPitch -=direction.z *deltaTimeMultiplier;

                m_cameraTargetPitch = ClampAngle(m_cameraTargetPitch, verticalMinRotation, verticalMaxRotation);
            }
        }
    }

    /// <summary>
    /// 基于玩家移动速度自动旋转相机
    /// 当玩家在地面上移动时，相机会根据玩家的速度方向来调整相机的偏航角度
    /// 营造相机跟随运动方向的效果
    /// </summary>
    protected virtual void HandleVelocityOrbit()
    {
        //玩家必须在地面上。防止在空中漂浮时乱转摄像机
        if (canOrbitWithVelocity&& player.isGrounded)
        {
            //将玩家速度转换到相机目标的本地坐标系中
            //localVelocity.x表示玩家相对相机前方的横向速度
            //localVelocity.z表示前后速度(前进后退)
            var localVelocity = m_target.InverseTransformDirection(player.velocity);
            
            //根据玩家左右速度调整相机偏航角度
            m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
        }
    }
    /// <summary>
    /// 处理相机高度偏移，使其根据玩家位置平滑跟随
    /// 相机在死区内保持稳定，当玩家脱离死区时相机缓慢跟随，避免画面抖动。
    /// 在空中时相机根据不同空中死区和平滑速度来调整高度，
    /// 保持一定得延迟感，让画面更自然
    /// </summary>
    protected virtual void HandleOffset()
    {
        var target = player.unsizedPosition + Vector3.up * heightOffset;

        var previousPosition = m_cameraTargetPosition;

        var targetHeight = previousPosition.y;

        if (player.isGrounded || VerticalFollowingStates())
        {
            if (target.y > previousPosition.y + verticalUpDeadZone)
            {
                //相机位移距离
                var offset = target.y - previousPosition.y - verticalUpDeadZone;
                //相机缓慢向上移动，增加量不超过每帧允许最大上升速度
                targetHeight += Mathf.Min(offset, maxVerticalSpeed * Time.deltaTime);
            }
            else if(target.y < previousPosition.y - verticalDownDeadZone)
            {
                var offset = target.y - previousPosition.y + verticalDownDeadZone;
                targetHeight += Mathf.Max(offset, -maxVerticalSpeed * Time.deltaTime);
            }
        }
        else if (target.y > previousPosition.y + verticalAirUpDeadZone)
        {
            //玩家在空中上升时
            var offset = target.y - previousPosition.y - verticalAirUpDeadZone;
            //缓慢向上跟随，空中通常比地面更慢，制造延迟感
            targetHeight += Mathf.Min(offset, maxAirVerticalSpeed * Time.deltaTime);
        }
        else if (target.y < previousPosition.y + verticalAirDownDeadZone)
        {
            var offset = target.y - previousPosition.y + verticalAirDownDeadZone;
            targetHeight += Mathf.Max(offset, -maxAirVerticalSpeed * Time.deltaTime);
        }
        //x,z始终跟随玩家
        //y使用平滑计算得targetHeight
        m_cameraTargetPosition = new Vector3(target.x,targetHeight,target.z);
    }
    protected virtual float ClampAngle(float angle, float min, float max)
    {
        if(angle <-360) angle += 360;
        if(angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
    protected virtual void LateUpdate()
    {
        HandleOrbit(); //输入环绕
        HandleVelocityOrbit();//速度驱动环绕
        HandleOffset();//高度跟随
        MoveTarget(); //更新
    }
}
