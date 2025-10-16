using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Game/Misc/Hazard")]
public class Hazard:MonoBehaviour,IEntityContact
{
    //是否实心
    public bool isSolid;
    //是否只能从上方对玩家造成伤害
    public bool damageOnlyFromAbove;
    //伤害值
    public int damage = 1;
    //当前collider
    protected Collider m_collider;

    protected virtual void Awake()
    {
        tag = GameTags.Hazard;
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = !isSolid;
    }

    protected virtual void TryToApplyDamage(Player player)
    {
        if (!damageOnlyFromAbove ||
            (player.velocity.y <= 0 && player.IsPointUnderStep(m_collider.bounds.max)))
        {
            player.ApplyDamage(damage,transform.position);
        }
    }

    public virtual void OnEntityContact(EntityBase entity)
    {
        if (entity is Player player)
        {
            TryToApplyDamage(player);
        }
    }
/// <summary>
/// 当有Collider停留在触发区域中执行
/// 主要用于非实心
/// </summary>
/// <param name="other"></param>
    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.Player))
        {
            if (other.TryGetComponent<Player>(out Player player))
            {
                TryToApplyDamage(player);
            }
        }
    }
}
