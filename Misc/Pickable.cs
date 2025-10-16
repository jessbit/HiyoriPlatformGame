using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Pickable:MonoBehaviour,IEntityContact{
    [Header("General Settings")]
    public Vector3 offset; //玩家手中的偏移位置
    public float releaseOffset = 0.5f;//释放时向前移动的距离

    [Header("Respawn Settings")]
    public bool autoRespawn; 
    public bool respawnOnHitHazards; //碰到Hazard是否重生
    public float respawnHeightLimit = -100; //超过某个高度是否重生

    [Header("Attack Settings")]
    public bool attackEnemies = true;
    public int damage = 1;
    public float minDamageSpeed = 5f; //超过速度阈值时才会攻击敌人

    [Space(15)]


    public UnityEvent onPicked;
    public UnityEvent onReleased;
    public UnityEvent onRespawn;

    protected Collider m_collider;
    protected Rigidbody m_rigidBody;

    //用于重生
    protected Vector3 m_initialPosition;
    protected Quaternion m_initialRotation;
    protected Transform m_initialParent;

    protected RigidbodyInterpolation m_interpolation; //保存插值模式(被拾取时关闭)

    public bool beingHold { get; protected set; }

    public virtual void PickUp(Transform slot)
    {
        if (!beingHold)
        {
            beingHold = true;
            transform.parent = slot;
            transform.localPosition = Vector3.zero + offset;
            transform.localRotation = Quaternion.identity;
            m_rigidBody.isKinematic = true;
            m_collider.isTrigger = true; //防止碰撞
            m_interpolation = m_rigidBody.interpolation;//保存插值模式
            m_rigidBody.interpolation = RigidbodyInterpolation.None;
            onPicked?.Invoke();
        }
    }
    public virtual void Release(Vector3 direction, float force)
    {
        if (beingHold)
        {
            transform.parent = null;
            transform.position += direction * releaseOffset;//稍微往释放方向偏移
            m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false; //恢复物理
            m_rigidBody.interpolation = m_interpolation; //恢复插值模式
            m_rigidBody.velocity = direction * force;
            onReleased?.Invoke();
        }
    }
    public void OnEntityContact(EntityBase entity)
    {
        if (attackEnemies && entity is Enemy &&
            m_rigidBody.velocity.magnitude > minDamageSpeed)
        {
            entity.ApplyDamage(damage, transform.position);
        }
    }

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_initialPosition = transform.localPosition;
        m_initialRotation = transform.localRotation;
        m_initialParent = transform.parent;
    }
    
    protected virtual void Update()
    {
        if (autoRespawn && transform.position.y <= respawnHeightLimit)
        {
            Respawn();
        }
    }
    /// <summary>
    /// 检查是否需要因为碰到危险物体而重生
    /// </summary>
    /// <param name="other"></param>
    protected virtual void EvaluateHazardRespawn(Collider other)
    {
        if (autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
        {
            Respawn();
        }
    }

    public virtual void Respawn()
    {
        m_rigidBody.velocity = Vector3.zero;
        transform.parent = m_initialParent;
        transform.SetPositionAndRotation(m_initialPosition,m_initialRotation);
        m_rigidBody.isKinematic =m_collider.isTrigger=beingHold = false;
        onRespawn?.Invoke();
    }
    protected virtual void OnTriggerEnter(Collider other) =>
        EvaluateHazardRespawn(other);

    protected virtual void OnCollisionEnter(Collision collision) =>
        EvaluateHazardRespawn(collision.collider);
}
