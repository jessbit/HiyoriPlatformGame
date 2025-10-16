using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager :MonoBehaviour
{
    //动作配置
    public InputActionAsset actions;
    //锁定移动方向时的时间戳(时间过小时禁止移动输入)
    protected float m_movementDirectionUnlockTime;
    //动作缓存
    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;
    protected InputAction m_crouch;
    protected InputAction m_dash;
    protected InputAction m_stomp;
    protected InputAction m_spin;
    protected InputAction m_airDive;
    protected InputAction m_dive;
    protected InputAction m_glide;
    protected InputAction m_grindBrake;
    protected InputAction m_releasedLedge;
    protected InputAction m_pause;
    protected InputAction m_run;
    protected InputAction m_pickAndDrop;
    protected Camera m_camera;
    protected const string k_mouseDeviceName = "Mouse";

    protected void Awake() => CacheActions();
    protected virtual void OnEnable()=>actions?.Enable();
    protected virtual void OnDisable()=>actions?.Disable();
    protected float? m_lastJumpTime;
    protected const float k_jumpBuffer = 0.15f;
    protected void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void Update()
    {
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime =Time.time;
        }
    }
    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
        m_look = actions["Look"];
        m_jump = actions["Jump"];
        m_crouch = actions["Crouch"];
        m_dash = actions["Dash"];
        m_stomp = actions["Stomp"];
        m_spin = actions["Spin"];
        m_airDive = actions["AirDive"];
        m_dive = actions["Dive"];
        m_glide = actions["Glide"];
        m_grindBrake = actions["Grind Brake"];
        m_releasedLedge = actions["ReleaseLedge"];
        m_pause = actions["Pause"];
        m_run = actions["Run"];
        m_pickAndDrop = actions["PickAndDrop"];
    }

    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();
        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }
        //死区处理，防止数值过小的抖动
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual bool IsLookingWithMouse()
    {
        if (m_look.activeControl == null)
        {
            return false;
        }

        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    public virtual void LockMovementDirection(float duration = 0.25f)
    {
        m_movementDirectionUnlockTime = Time.time + duration;
    }
    public virtual Vector3 GetMovementDirection()
    {
        if(Time.time<m_movementDirectionUnlockTime) return Vector3.zero;
        
        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }

    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin;
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadZone(axis.x, deadzone) : 0;
        axis.y =Mathf.Abs(axis.y) > deadzone ? RemapToDeadZone(axis.y, deadzone) : 0;
        return new Vector3(axis.x,0,axis.y);
    }
    /// <summary>
    /// 将输入值按给定死区重新映射到0-1
    /// </summary>
    /// protected float RemapToDeadZone(float value,float deadzone)=> (value - deadzone)/(1-deadzone);
    /// 过滤掉死区内的微小抖动和漂移（输入 → 0）。
    ///是不是输入搞反了？
    protected float RemapToDeadZone(float value,float deadzone)=> (value - (value < 0 ? -deadzone : deadzone))/(1-deadzone);

    public virtual Vector3 GetMovementCameraDirection()
    {
        var direction = GetMovementDirection();

        if (direction.sqrMagnitude > 0)
        {
            //以摄像机的Y轴构建目标真实旋转的旋转矩阵
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);
            direction = rotation * direction;
            direction = direction.normalized;
        }

        return direction;
    }

    public virtual bool GetJumpDown()
    {
        if (m_lastJumpTime != null &&
            Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }

        return false;
    }

    public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();
    public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();
    public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();
    public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();
    public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();
    public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();
    public virtual bool GetDive()=>m_dive.IsPressed();
    public virtual bool GetGlide()=>m_glide.IsPressed();
    public virtual bool GetRun()=>m_run.IsPressed();
    public virtual bool GetRunUp()=>m_run.WasReleasedThisFrame();
    public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();
    public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();
    public virtual bool GetReleaseLedgeDown() => m_releasedLedge.WasPressedThisFrame();
    public virtual bool GetGrindBrake() =>m_grindBrake.IsPressed();
}
