using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EntityVolumeEffector :MonoBehaviour
{
    //影响倍率
    public float velocityConversion = 1f;
    public float accelerationMultiplier = 1f;
    public float topSpeedMultiplier = 1f;
    public float decelerationMultiplier = 1f;
    public float turningDragMultiplier = 1f;
    public float gravityMultiplier = 1f;
    public float dashSpeedMultiplier = 1f;
    protected Collider m_collider;

    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out EntityBase entity))
        {
            entity.velocity *= velocityConversion;
            entity.accelerationMultiplier = accelerationMultiplier;
            entity.topSpeedMultiplier = topSpeedMultiplier;
            entity.decelerationMultiplier = decelerationMultiplier;
            entity.turningDragMultiplier = turningDragMultiplier;
            entity.gravityMultiplier = gravityMultiplier;
            entity.dashSpeedMultiplier = dashSpeedMultiplier;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out EntityBase entity))
        {
            entity.accelerationMultiplier = 1f;
            entity.topSpeedMultiplier = 1f;
            entity.decelerationMultiplier = 1f;
            entity.turningDragMultiplier = 1f;
            entity.gravityMultiplier = 1f;
            entity.dashSpeedMultiplier = 1f;
        }
    }
}