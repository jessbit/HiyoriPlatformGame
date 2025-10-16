using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityField :MonoBehaviour
{
    public float force = 75f;

    protected Collider m_collider;
    protected bool firstIn;

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.Player))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                //避免角色因地面检测而被拉住，确保能被场的力拉起
                if (player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                    player.verticalVelocity = Vector3.up *8;
                }
                player.velocity += transform.up * force * Time.deltaTime;
            }
        }
    }
    
}