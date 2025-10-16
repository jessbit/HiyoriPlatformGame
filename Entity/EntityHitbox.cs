using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("Game/Entity/EntityHitbox")]
public class EntityHitbox :MonoBehaviour
{
    [Header("Attack Settings")]
    public bool breakObjects;
    public int damage = 1;
    [Header("Rebound Settings")] //反弹效果
    public bool rebound;
    public float reboundMinForce = 10f;
    public float reboundMaxForce = 25f;
    [Header("Push Back Settings")]//击退效果
    public bool pushBack;
    public float pushBackMinMagnitude = 5f;
    public float pushBackMaxMagnitude = 10f;
    
    protected EntityBase m_entity;
    protected Collider m_collider;
    protected virtual void Start()
    {
        InitializeEntity();
        InitializeCollider();
    }

    protected virtual void InitializeEntity()
    {
        if (!m_entity)
        {
            m_entity=GetComponentInParent<EntityBase>();
        }
    }

    protected virtual void InitializeCollider()
    {
        m_collider=GetComponent<Collider>();
        m_collider.isTrigger=true;
    }

    protected void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
        HandleCustomCollision(other);//子类拓展
    }
    protected virtual void HandleCustomCollision(Collider other){}
    protected virtual void HandleCollision(Collider other)
    {
        if (other != m_entity.controller)
        {
            if (other.TryGetComponent(out EntityBase target))
            {
                HandleEntityAttack(target);
                HandleRebound();
                HandlePushBack();
            }
            else if (other.TryGetComponent(out Breakable breakable))
            {
                HandleBreakableObject(breakable);
            }
        }
    }

    protected virtual void HandleEntityAttack(EntityBase target)
    {
        target.ApplyDamage(damage,transform.position);
    }

    protected virtual void HandleRebound()
    {
        if (rebound)
        {
            var force = m_entity.velocity.y;
            force = Mathf.Clamp(force,reboundMinForce,reboundMaxForce);
            m_entity.verticalVelocity = Vector3.up * force;
        }
    }

    protected virtual void HandlePushBack()
    {
        if (pushBack)
        {
            var force = m_entity.lateralVelocity.magnitude;
            force = Mathf.Clamp(force,pushBackMinMagnitude,pushBackMaxMagnitude);
            m_entity.lateralVelocity = -transform.forward* force;
        }
    }

    protected virtual void HandleBreakableObject(Breakable breakable)
    {
        if (breakObjects)
        {
            breakable.Break();
        }
    }
}