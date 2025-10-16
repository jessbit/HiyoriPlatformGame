using UnityEngine;

public class PlayerStats : EntityStats<PlayerStats>
{
    [Header("General Stats")] 
    public float pushForce = 4f; //推动物体的力量
    public float snapForce = 15f;//将角色贴合到地面的吸附力
    public float slideForce = 10f;//下坡滑动的额外推力
    public float rotationSpeed = 970f;//角色旋转速度(度/s)
    public float gravity = 38f; //普通重力加速度
    public float fallGravity = 65f; //下落时额外重力加速度
    public float gravityTopSpeed = 50f; //重力作用下最大的下落速度
    [Header(("Motion Stats"))]
    //启用斜坡判断
    public bool applySlopeFactor = true;
    public float acceleration = 13f; //加速度
    public float deceleration = 28f; //减速度
    public float friction = 28f; //地面摩擦力
    public float slopeFriction = 18f;//坡面摩擦力
    public float topSpeed = 6f;//最高速度
    public float turningDrag = 28f; //转向阻力
    public float airAcceleration = 32f;//空中加速度
    public float brakeThreshold = -0.8f; //刹车判定阈值
    public float slopeUpwardForce = 15f;//上坡时额外推力
    public float slopeDownwardForce = 18f;//下坡时额外推力
    [Header("Running Stats")]
    public float runningAcceleration = 16f;
    public float runningTopSpeed = 9f;
    public float runningTurningDrag = 14f;

    [Header(("Jump Stats"))]
    public int multiJumps = 1;
    public float coyoteJumpThreshold = 0.15f;
    public float maxJumpHeight = 17f;
    public float minJumpHeight = 10f;
    [Header("Hurt Stats")]
    public float hurtUpwardForce = 10f; //受伤时向上的力
    public float hurtBackwardsForce = 5f;//受伤时向后的力
    [Header("Crouch Stats")]
    public float crouchHeight = 1f;
    public float crouchFriction = 10f;
    [Header("Crawling Stats")]
    public float crawlingAcceleration = 8f;
    public float crawlingFriction = 32f;
    public float crawlingTopSpeed = 2.5f;
    public float crawlingTurningSpeed = 3f; //转向阻力
    [Header("Backflip Stats")]
    public bool canBackflip = true;
    public bool backflipLockMovement = true;
    public float backflipAirAcceleration=12f;
    public float backflipTurningDrag = 2.5f;
    public float backflipTopSpeed = 7.5f;
    public float backflipJumpHeight = 23f;
    public float backflipGravity = 35f;
    public float backflipBackwardForce = 4f; 
    public float backflipBackwardTurnForce = 8f;
    [Header("Dash Stats")]
    public bool canAirDash = true;
    public bool canGroundDash = true;
    public float dashForce = 25f;
    public float dashDuration = 0.3f;
    public float groundDashCoolDown = 0.5f;
    public float allowedAirDashes = 1;
    [Header("Stomp Attack Stats")]
    public bool canStompAttack = true;
    public float stompDownwardForce = 20f;
    public float stompAirTime = 0.8f;
    public float stompGroundTime = 0.5f;
    public float stompGroundLeapHeight = 10f;
    [Header("Spin Stats")]
    public bool canSpin = true;
    public bool canAirSpin = true;
    public float spinDuration = 0.5f;
    public float airSpinUpwardForce = 10f; //空中旋转攻击时的上升力
    public int allowedAirSpins = 1;
    [Header("Pick'n Throw Stats")]
    public bool canPickUp = true;
    public bool canPickUpOnAir = false;
    public bool canJumpWhileHolding = true;
    public float pickDistance = 0.5f;
    public float throwVelocityMultiplier = 1.5f;
    [Header("Air Dive Stats")]
    public bool canAirDive = true;
    public bool applyDiveSlopeFactor = true; //是否考虑坡度
    public float airDiveForwardForce = 16f;
    public float airDiveFriction = 32f;
    public float airDiveSlopeFriction = 12f;//坡地摩擦
    public float airDiveSlopeUpwardForce = 35f;//上坡时额外推力
    public float airDiveSlopeDownwardForce = 40f;//下坡时额外推力
    public float airDiveGroundLeapHeight = 10f;//俯冲落地后的跳跃高度
    public float airDiveRotationSpeed = 45f;//俯冲旋转速度
    [Header("Swiming Stats")]
    public float waterConversion = 0.35f;
    public float waterRotationSpeed = 360f;
    public float waterUpwardsForce = 8f;//浮力
    public float waterJumpHeight = 15f;
    public float waterTurningDrag = 2.5f;
    public float swimAcceleration = 4f;
    public float swimDeceleration = 3f;
    public float swimTopSpeed = 4f;
    public float swimDiveForce = 15f;
    [Header("Gliding Stats")]
    public bool canGlide = true;
    public float glidingGravity = 10f;
    public float glidingMaxFallSpeed = 2f;
    public float glidingTurningDrag = 8f;
    [Header("Wall Drag Stats")]
    public bool canWallDrag = true;
    public bool wallJumpLockMovement = true;
    public LayerMask wallDragLayers;
    public Vector3 wallDragSkinOffset; //判定偏移
    public float wallDragGravity = 12f;
    public float wallJumpDistance = 8f;
    public float wallJumpHeight = 15f;
    [Header("Pole Climb Stats")]
    public bool canPoleClimb = true;
    public Vector3 poleClimbSkinOffset;
    public float climbUpSpeed = 3f;
    public float climbDownSpeed = 8f;
    public float climbRotationSpeed = 2f;
    public float poleJumpDistance = 8f;
    public float poleJumpHeight = 15f;
    [Header("Rail Grinding Stats")]
    public bool useCustomCollision=true;
    public float grindRadiusOffset = 0.26f;//贴合轨道的半径偏移
    public float minGrindInitialSpeed = 10f;
    public float minGrindSpeed = 5f;
    public float grindTopSpeed = 25f;
    public float grindDownSlopeForce = 40f;//下坡时推力
    public float grindUpSlopeForce = 30f;//上坡时推力
    [Header("Rail Grinding Brake")]
    public bool canGrindBrake = true;
    public float grindBrakeDeceleration = 10f;
    [Header("Rail Grinding Dash Stats")]
    public bool canGrindDashDash = true;
    public bool applyGrindingSlopeFactor=true;//是否考虑坡度
    public float grindDashCoolDown = 0.5f;//冲刺冷却时间
    public float grindDashForce = 25f; 
    [Header("Ledge Hanging Stats")]
    public bool canLedgeHang = true;
    public LayerMask ledgeHangingLayers;
    public Vector3 ledgeHangingSkinOffset;
    public float ledgeMaxForwardDistance = 0.25f;
    public float ledgeMaxDownwardOffset = 0.15f;
    public float ledgeSideMaxDistance = 0.5f;
    public float ledgeSideHeightOffset = 0.15f;
    public float ledgeSideCollisionRadius = 0.25f;
    public float ledgeMovementSpeed = 1.5f;
    [Header("Ledge Climbing Stats")]
    public bool canClimbLedges = true;
    public LayerMask ledgeClimbLayers;
    public Vector3 ledgeClimbSkinOffset;
    public float ledgeClimbingDuration = 1f;
}
