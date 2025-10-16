using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.XR;

public abstract class EntityBase : MonoBehaviour
{
    protected Collider[] m_contactBuffer = new Collider[10];
    public EntityEvents entityEvents;
    //忽略碰撞器缩放的实体位置  
    public Vector3 unsizedPosition => position-transform.up*height*0.5f+transform.up*originalHeight*0.5f;
    public bool isGrounded { get; protected set; } = true;
    public bool onRails { get; protected set; }
    public SplineContainer rails { get; protected set; }
    protected readonly float m_groundOffset = 0.1f;//地面检测偏移
    public CharacterController controller { get; protected set; }
    public float originalHeight{get; protected set;}
    public Vector3 lastPosition { get; protected set; }
    public float positionDelta{get; protected set;}//当前位置和上一帧位置距离
    public float lastGroundTime { get; protected set; }
    public float groundAngle { get; protected set; }
    public Vector3 groundNormal { get; protected set; }
    public Vector3 localSlopeDirection { get; protected set; }
    public RaycastHit groundHit;
    protected CapsuleCollider m_collider;
    protected Rigidbody m_rigidbody;
    public float height =>controller.height;
    public float radius =>controller.radius;
    public Vector3 center =>controller.center;
    public Vector3 position => transform.position+center;
    //脚底位置
    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);
    
    //倍率
    public float accelerationMultiplier { get; set; } = 1f;
    public float gravityMultiplier { get; set; } = 1f;
    public float topSpeedMultiplier { get; set; } = 1f;
    public float turningDragMultiplier { get; set; } = 1f;
    public float decelerationMultiplier { get; set; } = 1f;
    public float dashSpeedMultiplier { get; set; } = 1f;
    public float m_slopingGroundAngle = 20f; //滑坡角
    public Vector3 velocity { get; set; }
    public Vector3 lateralVelocity { 
        get{return new Vector3(velocity.x,0, velocity.z);}
        set{velocity = new Vector3(value.x,velocity.y, value.z);}
    }
    //垂直速度
    public Vector3 verticalVelocity { 
        get{return new Vector3(0,velocity.y,0);}
        set{velocity = new Vector3(velocity.x,value.y, velocity.z);}
    } 
    public virtual void ResizeCollider(float height)
    {
        var delta = height - this.height;
        controller.height = height;
        //调整控制器中心位置，根据高度变化自动平移
        controller.center +=Vector3.up *delta * 0.5f;
    }
    //判断实体是否在斜坡上
    public virtual bool OnSlopingGround()
    {
        if (isGrounded && groundAngle > m_slopingGroundAngle)
        {
            if (Physics.Raycast(transform.position, -transform.up, out var hit, height * 2f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return Vector3.Angle(hit.normal, Vector3.up) > m_slopingGroundAngle;
            else
                return true;
        }

        return false;
    }
    public virtual bool IsPointUnderStep(Vector3 point) =>stepPosition.y >= point.y;
    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        //球形检测有效距离
        var castDistance = Mathf.Abs(distance - radius);
        return Physics.SphereCast(position,radius,direction,
            out hit, castDistance, layer,queryTriggerInteraction);
    }
    public virtual bool SphereCast(Vector3 direction, float distance,
            int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        //调用带有返回检测信息的方法，并忽略返回的hit信息
        return SphereCast(direction,distance,out _,layer, queryTriggerInteraction);
    }

    public virtual bool CapsuleCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        return CapsuleCast(direction,distance,out _,layer, queryTriggerInteraction);
    }
    public virtual bool CapsuleCast(Vector3 direction, float distance, out RaycastHit hit,
        int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        var origin = position - direction * radius + center;
        //计算偏移量，调整胶囊体的上下半部分，使碰撞器的中心处于正确位置
        var offset = transform.up * (height * 0.5f - radius);
        var top = origin + offset;
        var bottom = origin - offset;
        return Physics.CapsuleCast(top,bottom,radius,direction,
            out hit,distance+radius,layer,queryTriggerInteraction);
    }

    /// <summary>
    /// 查找覆盖范围内的胶囊碰撞体
    /// </summary>
    /// <param name="result"></param>
    /// <param name="skinOffset"></param>
    /// <returns></returns>
    public virtual int OverlapEntity(Collider[] result, float skinOffset = 0)
    {
        var contactOffset = skinOffset + controller.skinWidth + Physics.defaultContactOffset;
        //计算重叠半径(包括胶囊碰撞体的半径和接触偏移量)
        var overlapsRadius = radius + contactOffset;
        //顶部和底部的偏移量
        var offset = (height + contactOffset) * 0.5f - overlapsRadius;
        var top = position + Vector3.up * offset;
        var bottom = position + Vector3.down * offset;
        return Physics.OverlapCapsuleNonAlloc(top, bottom, overlapsRadius, result);
    }
    
    public virtual void ApplyDamage(int damage, Vector3 origin) { }
    protected virtual void OnUpdate(){}
}
//泛型约束
public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    //状态管理器
    public EntityStateManager<T> states { get; protected set; }

    protected virtual void Awake()
    {
        InitializeController();
        InitializeStateManager();
    }

    protected virtual void InitializeCollider()
    {
        m_collider =gameObject.AddComponent<CapsuleCollider>();
        m_collider.height = controller.height;
        m_collider.radius = controller.radius;
        m_collider.center = controller.center;
        //不产生物理碰撞，只检测
        m_collider.isTrigger = true;
        //默认禁用，只在需要时启用
        m_collider.enabled = false;
    }

    protected virtual void InitializeRigidbody()
    {
        m_rigidbody =gameObject.AddComponent<Rigidbody>();
        //启用运动学刚体，不受重力和物理力影响
        m_rigidbody.isKinematic = true;
    }
    protected virtual void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        if (!controller)
        {
            controller =gameObject.AddComponent<CharacterController>();
        }
        //skinWidth表示碰撞器表面实际碰撞检测边界的距离
        controller.skinWidth = 0.005f;
        //minMoveDistance 为最小移动距离(设为0表示即使移动非常小也会被检测)
        controller.minMoveDistance = 0;
        //记录角色控制初始高度(用于复位和高度调整)
        originalHeight=controller.height;
    }
    //处理状态机步进逻辑
    protected virtual void InitializeStateManager()=> states = GetComponent<EntityStateManager<T>>();
    
    /// <summary>
    /// 计算玩家移动时的速度
    /// </summary>
    /// <param name="direction">方向</param>
    /// <param name="turningDrag">转角阻力</param>
    /// <param name="acceleration">加速度</param>
    /// <param name="topSpeed">最高速度</param>
    public virtual void Accelerate (Vector3 direction, float turningDrag, float acceleration, float topSpeed)
    {
        if (direction.sqrMagnitude > 0)
        {
            //当前速度在目标方向上的投影速度
            var speed = Vector3.Dot(direction, lateralVelocity);
            //计算当前速度在目标方向上的向量
            var velocity = direction * speed;
            //计算当前速度中垂直于目标方向的部分(转向速度)
            var turningVelocity = lateralVelocity -velocity;
            //计算转向阻力对应的速度变化量(根据转向阻力系数喝时间增量)
            var turningDelta = turningDrag *turningDragMultiplier * Time.deltaTime;
            //最大允许速度
            var targetTopSpeed = topSpeed * topSpeedMultiplier;
            
            //速度未达最大速度或当前速度与目标方向相反则加速
            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                speed += acceleration * accelerationMultiplier;
                speed = Mathf.Clamp(speed,-targetTopSpeed,targetTopSpeed);
            }
            
            velocity = direction * speed;
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            lateralVelocity = velocity + turningVelocity;
        }
    }

    public virtual void Decelerate(float deceleration)
    {
        //计算本帧的减速度
        var delta = deceleration * decelerationMultiplier * Time.deltaTime;
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }
    public virtual void SlopeFactor(float upwardForce, float downwardForce)
    {
        if (!isGrounded || !OnSlopingGround()) return;

        var factor = Vector3.Dot(Vector3.up, groundNormal);
        var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
        var multiplier = downwards ? downwardForce : upwardForce;
        var delta = Mathf.Sign(factor) * multiplier * Time.deltaTime;
        lateralVelocity += localSlopeDirection * delta;
    }
    //根据hit点判断是否着陆
    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
        //slopeLimit是坡度最大限制角度
        return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }
    protected virtual void HandleSlopeLimit(RaycastHit hit) { }
    protected virtual void HandleHighLedge(RaycastHit hit) { }
    protected virtual void EnterGround(RaycastHit hit)
    {
        if (!isGrounded)
        {
            groundHit = hit;
            isGrounded = true;
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    protected virtual void ExitGround()
    {
        if (isGrounded)
        {
            isGrounded = false;
            transform.parent = null;
            lastGroundTime = Time.time;
            //限制垂直速度,如果向下运动，不改变，如果有向上的速度则保留
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    protected virtual void UpdateGround(RaycastHit hit)
    {
        if (isGrounded)
        {
            groundHit = hit;
            //记录地面法线，用于计算坡度方向
            groundNormal = groundHit.normal;
            //计算坡度角(与世界y夹角)
            groundAngle = Vector3.Angle(Vector3.up, groundNormal);
            //计算本地的坡度方向
            localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            //若地面是平台，让角色成为平台的子物体，跟随平台移动
            transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
        }
    }

    public virtual void Gravity(float gravity)
    {
        if (!isGrounded)
        {
            verticalVelocity += Vector3.down * gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    public virtual bool FitsIntoPosition(Vector3 position)
    {
        var radius =controller.radius -controller.skinWidth;
        var offset = height * 0.5f - radius;
        var top = position + Vector3.up * offset;
        var bottom = position - Vector3.up * offset;
        
        return !Physics.CheckCapsule(top,bottom,radius,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }
    public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
    {
        if (direction != Vector3.zero)
        {
            var rotation = transform.rotation;
            //每帧的旋转角度
            var rotationDelta = degreesPerSecond * Time.deltaTime;
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(rotation,target,rotationDelta);
        }
    }

    public virtual void FaceDirection(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0)
        {
            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = rotation;
        }
    }
    /// <summary>
    /// 将角色吸附到地面，防止悬空
    /// </summary>
    public virtual void SnapToGround(float force)
    {
        //接触地面，且速度向下才生效
        if (isGrounded && (verticalVelocity.y <= 0))
        {
            //将垂直速度设置为一个恒定向下的力(防止落地浮空)
            verticalVelocity = Vector3.down * force;
        }
    }
    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }
        transform.position += velocity * Time.deltaTime;
    }
    protected virtual void HandleStates() => states.Step();

    protected virtual void HandleGround()
    {
        if (onRails) return;
        var distance = (height * 0.5f) + m_groundOffset;
        if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            if (!isGrounded)
            {
                if (EvaluateLanding(hit))
                {
                    EnterGround(hit);
                }
            }
            else if (IsPointUnderStep(hit.point))
            {
                UpdateGround(hit);
                if (Vector3.Angle(hit.normal, Vector3.up) >= controller.slopeLimit)
                {
                    HandleSlopeLimit(hit);
                }
            }
        }
        else
        {
            ExitGround();
        }
    }

    protected virtual void OnContact(Collider other)
    {
        if (other)
        {
            states.OnContact(other);
        }
    }
    public virtual void HandleContacts()
    {
        //检测与其他实体的重叠，把结果存入m_contactBuffer
        var overlaps = OverlapEntity(m_contactBuffer);
        for (int i = 0; i < overlaps; i++)
        {
            if (!m_contactBuffer[i].isTrigger && m_contactBuffer[i].transform != transform)
            {
                OnContact(m_contactBuffer[i]);

                var listeners = m_contactBuffer[i].GetComponents<IEntityContact>();
                foreach (var contact in listeners)
                {
                    //依次回调实现双向交互
                    contact.OnEntityContact((T)this);
                }

                if (m_contactBuffer[i].bounds.min.y > controller.bounds.max.y)
                {
                    //限制向上速度防止向上穿
                    verticalVelocity = Vector3.Min(verticalVelocity, Vector3.zero);
                }
            }
        }
    }

    protected virtual void EnterRail(SplineContainer rails)
    {
        if (!onRails)
        {
            onRails = true;
            this.rails = rails;
            entityEvents.OnRailsEnter.Invoke();
        }
    }

    public virtual void ExitRail()
    {
        if (onRails)
        {
            onRails = false;
            entityEvents.OnRailsExit.Invoke();
        }
    }

    public virtual void UseCustomCollision(bool value)
    {
        controller.enabled =!value;
        if (value)
        {
            InitializeCollider();
            InitializeRigidbody();
        }
        else
        {
            Destroy(m_collider);
            Destroy(m_rigidbody);
        }
    }
    protected virtual void HandleSpline()
    {
        var distance = (height * 0.5f) + height * 0.5f;
        if (SphereCast(-transform.up, distance, out var hit) &&
            hit.collider.CompareTag(GameTags.InteractiveRail))
        {
            if (!onRails && verticalVelocity.y <= 0)
            {
                EnterRail(hit.collider.GetComponent<SplineContainer>());
            }
        }
        else
        {
            ExitRail();
        }
    }
    protected virtual void Update()
    {
        if (controller.enabled || m_collider!=null)
        {
            HandleStates();
            HandleController();
            HandleSpline();
            HandleGround();
            HandleContacts();
            OnUpdate();
        }
    }

    protected virtual void HandlePosition()
    {
        positionDelta = (position - lastPosition).magnitude;
        lastPosition = position;
    }
    //收尾修正
    protected virtual void LateUpdate()
    {
        if (controller.enabled)
        {
            HandlePosition();
        }
    }
}
