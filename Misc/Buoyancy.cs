using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Buoyancy :MonoBehaviour
{
    public float force = 10f;

    protected Rigidbody m_rigidbody;

    protected virtual void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(GameTags.VolumeWater))
        {
            //判断物体是否部分或完全在水体内部
            if (transform.position.y < other.bounds.max.y)
            {
                //计算浮力
                var multiplier = Mathf.Clamp01((other.bounds.max.y - transform.position.y));
                var buoyancy = Vector3.up * force * multiplier;
                m_rigidbody.AddForce(buoyancy);
            }
        }
    }
}
