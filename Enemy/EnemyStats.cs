using UnityEngine;

public class EnemyStats : EntityStats<EnemyStats>
{
    [Header("General Stats")] public float gravity = 35f;
    public float SnapForce = 15f;
    public float rotationSpeed = 970f;
    public float deceleration = 28f;
    public float friction = 16f;
    public float turningDrag = 28f;
    [Header("Follow Stats")] public float followAcceleration = 10f;
    public float followTopSpeed = 4f;
    [Header(("View Stats"))]
    public float spotRange = 5f;
    public float viewRange = 8f;
    [Header("Contact Attack Stats")]
    public bool canAttackOnContact = true;
    public bool contactPushback = true;
    //接触检测的偏移量
    public float contactOffset = 0.15f;
    public int contactDamage = 1;
    public float contactPushBackForce = 18f;
    //接触攻击中容忍玩家踩踏的距离(垂直容差)
    public float contactSteppingTolerance = 0.1f;
    [Header("Waypoint Stats")]
    //是否朝向当前路径点旋转
    public bool faceWaypoint = true;
    //多近到达
    public float waypointMinDistance = 0.5f;
    public float waypointAcceleration = 10f;
    public float waypointTopSpeed = 2f;
    
}