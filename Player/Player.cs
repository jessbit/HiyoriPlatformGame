using System;
using UnityEngine;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvents;
    public PlayerInputManager inputs {get; protected set;}
    public PlayerStatsManager stats {get; protected set;}
    public int jumpCounter { get; protected set; }
    public int airDashCounter { get; protected set; }
    public float lastDashTime { get; protected set; }
    public float airSpinCounter { get; protected set; }
    public bool onWater { get; protected set; }
    public Health health { get; protected set; }

    public virtual bool isAlive => !health.isEmpty;
    
    //皮肤初始位置与旋转(用于恢复外观)
    protected Vector3 m_skinInitialPosition = Vector3.zero;
    protected Quaternion m_skinInitialRotation = Quaternion.identity;
    
    public Transform skin;//玩家的皮肤外观transform，用于重置姿态
    public bool holding { get; protected set; }
    public Vector3 lastWallNormal { get; protected set; }
    //从水中出来的微笑偏移
    protected const float k_waterExitOffset = 0.25f;
    public Collider water { get; protected set; }
    public Pole pole { get; protected set; }
    public Pickable pickable { get; protected set; }
    public Transform pickableSlot;
    
    protected Vector3 m_respawnPosition;
    protected Quaternion m_respawnRotation;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStats();
        InitializeHealth();
        InitializeTag();
        InitializeRespawn();
        entityEvents.OnGroundEnter.AddListener(()=>
        {
            ResetJumps();
            ResetAirDash();
            ResetAirSpins();
        });
        entityEvents.OnRailsEnter.AddListener(() =>
        {
            ResetJumps();
            ResetAirSpins();
            ResetAirDash();
            StartGrind();
        });
    }

    protected virtual void StartGrind() => states.Change<RailGrindPlayerState>();
    protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
    protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();
    protected virtual void InitializeHealth() => health = GetComponent<Health>();
    protected virtual void InitializeTag() => tag =GameTags.Player;

    protected virtual void InitializeRespawn()
    {
        m_respawnPosition = transform.position;
        m_respawnRotation = transform.rotation;
    }
    /// <summary>
    /// 设置玩家重生点
    /// </summary>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public virtual void SetRespawn(Vector3 position, Quaternion rotation)
    {
        m_respawnPosition = position;
        m_respawnRotation = rotation;
    }
    /// <summary>
    /// 玩家重生
    /// </summary>
    public virtual void Respawn()
    {
        health.Reset();
        transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
        states.Change<IdlePlayerState>();
    }
    /// <summary>
    ///  使玩家朝方向加速
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Accelerate(Vector3 direction)
    {
        var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;
        var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;
        
        Accelerate(direction,turningDrag,finalAcceleration,topSpeed);
    }
    public virtual void CrawlingAccelerate(Vector3 direction) => Accelerate(direction,stats.current.crawlingTurningSpeed,stats.current.crawlingAcceleration,stats.current.crawlingTopSpeed);
    
    public virtual void BackflipAcceleration()
    {
        var direction = inputs.GetMovementCameraDirection();
        Accelerate(direction,stats.current.backflipTurningDrag,stats.current.crawlingAcceleration,stats.current.crawlingTopSpeed);
    }
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);
    public virtual void Friction()
    {
        if (OnSlopingGround()) Decelerate(stats.current.slopeFriction);
        else Decelerate(stats.current.friction);
    }

    public virtual void FallWithWall()
    {
        var distance = height * 0.5f+m_groundOffset;
        if (SphereCast(-transform.up,distance, out var hit))
        {
            //平面方向夺走的速度
            Vector3 velocityNormal = Vector3.Dot(verticalVelocity, hit.normal) * hit.normal;
            // 切线方向分量（沿着斜面滑动）
            Vector3 velocityTangent = verticalVelocity - velocityNormal;
            verticalVelocity = velocityTangent;
            // lateralVelocity 如果需要的话，可以从 velocityTangent 中提取水平分量
            lateralVelocity = new Vector3(velocityTangent.x, 0, velocityTangent.z);
        }
    }
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = inputs.GetMovementCameraDirection();//输入相对相机的方向
        Accelerate(inputDirection);
    }
    public virtual void Gravity()
    {
        if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            //上升时用普通重力，下落时用更强的重力
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;
            speed = Mathf.Max(speed,-stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
    public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);
    public virtual void ResetJumps() => jumpCounter = 0;
    public virtual void SetJumps(int amount) => jumpCounter = amount;

    public virtual void ResetSkinParent()
    {
        if (skin)
        {
            skin.parent = transform;
            skin.localPosition = m_skinInitialPosition;
            skin.localRotation = m_skinInitialRotation;
        }
    }

    public virtual void SetSkinParent(Transform parent)
    {
        if (skin)
        {
            skin.parent=parent;
        }
    }
    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }
    public virtual void Jump()
    {
        //多段跳判定
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        //土狼跳判定
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
        var holdJump = !holding || stats.current.canJumpWhileHolding;
        if ((isGrounded || canMultiJump || canCoyoteJump||onRails)&&holdJump)
        {
            if (inputs.GetJumpDown())
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }

    public virtual void Jump(float height)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        states.Change<FallPlayerState>();
        playerEvents.OnJump?.Invoke();
    }

    public virtual void Backflip(float force)
    {
        if (stats.current.canBackflip && !holding)
        {
            verticalVelocity = Vector3.up *stats.current.backflipJumpHeight;//上跳力
            lateralVelocity = -transform.forward * force;//向后推力
            states.Change<BackflipPlayerState>();
            playerEvents.OnBackflip?.Invoke();
        }
    }
    public virtual void RegularSlopeFactor()
    {
        if (stats.current.applySlopeFactor)
            SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
    }
    public virtual void ResetAirDash()=>airDashCounter = 0;
    public virtual void ResetAirSpins()=>airSpinCounter = 0;
    public virtual void Dash()
    {
        var canAirDash = stats.current.canAirDash && !isGrounded &&
                         airDashCounter < stats.current.allowedAirDashes;
        var canGroundDash = stats.current.canGroundDash && isGrounded &&
                            Time.time - lastDashTime > stats.current.groundDashCoolDown;
        if (inputs.GetDashDown() && (canAirDash || canGroundDash))
        {
            if (!isGrounded) airDashCounter++;
            lastDashTime = Time.time;
            states.Change<DashPlayerState>();
        }
    }
    public virtual void ApplyDamage(int amount, Vector3 origin)
    {
        if (!health.isEmpty && !health.recovering)
        {
            health.Damage(amount);
            //计算受击方向
            var damageDir=origin -transform.position;
            damageDir.y = 0; //忽略垂直方向
            damageDir = damageDir.normalized;
            //面朝攻击方向
            FaceDirection(damageDir);
            lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;
            if (!onWater) //不在水中击飞向上并进入受击状态
            {
                verticalVelocity=Vector3.up*stats.current.hurtUpwardForce;
                states.Change<HurtPlayerState>();
            }
            
            playerEvents.OnHurt?.Invoke();//触发受伤事件
            if (health.isEmpty)
            {
                Throw();
                playerEvents.OnDie?.Invoke();
            }
        }
    }

    public virtual void StompAttack()
    {
        if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
        {
            states.Change<StompPlayerState>();
        }
    }

    public virtual void Spin()
    {
        var canAirSpin = (isGrounded || stats.current.canAirSpin) && airSpinCounter < stats.current.allowedAirSpins;
        if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
        {
            if (!isGrounded)
            {
                airSpinCounter++;
            }
            states.Change<SpinPlayerState>();
            playerEvents.OnSpin?.Invoke();
        }
    }

    public virtual void AirDive()
    {
        if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
        {
            states.Change<AirDivePlayerState>();
            playerEvents.OnAirDive?.Invoke();
        }
    }

    public virtual void WaterAcceleration(Vector3 direction) =>
        Accelerate(direction, stats.current.waterTurningDrag, stats.current.swimAcceleration,
            stats.current.swimTopSpeed);

    public virtual void WaterFaceDirection(Vector3 direction) =>
        FaceDirection(direction, stats.current.waterRotationSpeed);
    public virtual void EnterWater(Collider water)
    {
        if (!onWater && !health.isEmpty)
        {
            //丢掉手上的物体
            //Throw();
            onWater = true;
            this.water = water;
            states.Change<SwimPlayerState>();
        }
    }
    public virtual void ExitWater()
    {
        if (onWater)
        {
            onWater=false;
        }
    }

    public virtual void Glide()
    {
        if(!isGrounded&&inputs.GetGlide()&&
           verticalVelocity.y <=0 && stats.current.canGlide)
            states.Change<GlidingPlayerState>();
    }

    public virtual void GlidingAcceleration(Vector3 direction) =>
        Accelerate(direction, stats.current.glidingTurningDrag, stats.current.airAcceleration, stats.current.topSpeed);

    public virtual void WallDrag(Collider other)
    {
        if (stats.current.canWallDrag && velocity.y<=0&&
            !holding && !other.TryGetComponent<Rigidbody>(out _))
        {
            if (CapsuleCast(transform.forward, 0.25f, out var hit,
                    stats.current.wallDragLayers))
            {
                if(hit.collider.CompareTag(GameTags.Platform))
                    transform.parent = hit.transform;
                lastWallNormal = hit.normal;
                states.Change<WallDragPlayerState>();
            }
        }
    }

    public virtual void DirectionalJump(Vector3 direction, float height, float distance)
    {
        jumpCounter++;
        verticalVelocity = Vector3.up * height;
        lateralVelocity = direction*distance;
        playerEvents.OnJump?.Invoke();
    }

    public virtual void GrabPole(Collider other)
    {
        if (stats.current.canPoleClimb && velocity.y <= 0
                                       && !holding && other.TryGetComponent(out Pole pole))
        {
            this.pole = pole;
            states.Change<PoleClimbingPlayerState>();
        }
    }
    /// <summary>
    ///  检测角色前方是否有可挂的Ledge
    /// </summary>
    /// <param name="forwardDistance"></param>
    /// <param name="downwardDistance"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public virtual bool DetectingLedge(float forwardDistance, float downwardDistance,out RaycastHit hit)
    {
        //用于避免射线检测时因浮点误差避免卡进墙里的自定义修正
        var contactOffset = Physics.defaultContactOffset + positionDelta;
        //前方检测最大长度 ->角色半径+额外前探距离
        var ledgeMaxDistance = radius + forwardDistance;
        //从角色中心向上的偏移，代表检测ledge高度起点
        //半个角色高度+接触修正
        var ledgeHeightOffset = height * 0.5f + contactOffset;
        //角色局部坐标系中的xx向量
        //表示从当前位置向上移动到检测ledge顶部的点
        var upwardOffset = transform.up * ledgeHeightOffset;
        var forwardOffset = transform.forward * ledgeMaxDistance;
        //检测前方或者上方是否有阻挡
        //从角色上偏移点往前打射线，检测角色头前方是否有障碍，如果头不超过平台，无法悬挂
        //从前方一点点往上打射线，检测上方是否有阻碍
        //若阻挡，角色不能挂边
        if (Physics.Raycast(position + upwardOffset, transform.forward, ledgeMaxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) ||
            Physics.Raycast(position + forwardOffset, transform.up, ledgeHeightOffset,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            hit = new();
            return false;
        }
        //向下检测
        //起点为头顶高度，往前探到ledge边缘的位置
        var origin = position + upwardOffset + forwardOffset;
        //向下检测的长度
        var distance = downwardDistance + contactOffset;
        return Physics.Raycast(origin, Vector3.down, out hit, distance,
            stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);
    }
    public virtual void LedgeGrab()
    {
        if(stats.current.canLedgeHang&&velocity.y<0&&!holding&&
           states.ContainsStateOfType(typeof(LedgeHangingPlayerState))&&
           DetectingLedge(stats.current.ledgeMaxForwardDistance,stats.current.ledgeMaxDownwardOffset,out var hit))
        {
            //悬挂的部位不可能光滑
            if (!(hit.collider is CapsuleCollider) && !(hit.collider is SphereCollider))
            {
                var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
                var lateraOffset = transform.forward * ledgeDistance;
                //角色中心到脚底的偏移量
                var verticalOffset = Vector3.down * height * 0.5f - center;
                
                velocity = Vector3.zero;
                transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
                var desination = hit.point - lateraOffset + verticalOffset;
                /*if (transform.parent)
                {
                    Vector3 localSpaceMovement = transform.parent.rotation * desination;
                    transform.localPosition = localSpaceMovement;
                }*/
                transform.position = desination;
                states.Change<LedgeHangingPlayerState>();
                playerEvents.OnLedgeGrabbed?.Invoke();
            }  
        }
    }
    protected void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            if (!onWater && other.bounds.Contains(unsizedPosition))
            {
                EnterWater(other);
            }
            else if (onWater)
            {
                var exitPoint = position + Vector3.down * k_waterExitOffset;

                if (!other.bounds.Contains(exitPoint))
                {
                    ExitWater();
                }
            }
        }
    }

    public virtual void PushRigidbody(Collider other)
    {
        //防止在台阶上
        if (!IsPointUnderStep(other.bounds.max) &&
            other.TryGetComponent(out Rigidbody rigidbody))
        {
            var force = lateralVelocity * stats.current.pushForce;
            rigidbody.velocity += force / rigidbody.mass * Time.deltaTime;
        }
    }

    public virtual void PickAndThrow()
    {
        if (stats.current.canPickUp && inputs.GetPickAndDropDown())
        {
            if (!holding)
            {
                if (CapsuleCast(transform.forward,
                        stats.current.pickDistance, out var hit))
                {
                    if (hit.transform.TryGetComponent(out Pickable pickable))
                    {
                        PickUp(pickable);
                    }
                }
            }
            else
            {
                Throw();
            }
        }
    }

    public virtual void PickUp(Pickable pickable)
    {
        if (!holding && (isGrounded || stats.current.canPickUpOnAir))
        {
            holding = true;
            this.pickable = pickable;
            pickable.PickUp(pickableSlot);//把物品附着到拾取点
            pickable.onRespawn.AddListener(RemovePickable); //监听物体的重生事件，如果重生就清楚引用
            playerEvents.OnPickUp?.Invoke();
        }
    }

    public virtual void RemovePickable()
    {
        if (holding)
        {
            pickable = null;
            holding = false;
        }
    }
    public virtual void Throw()
    {
        if (holding)
        {
            //投掷力与玩家水平速度有关
            var force = lateralVelocity.magnitude * stats.current.throwVelocityMultiplier;
            pickable.Release(transform.forward, force);
            pickable = null;
            holding = false;
            playerEvents.OnThrow?.Invoke();
        }
    }
}
