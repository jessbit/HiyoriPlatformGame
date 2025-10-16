using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class Pole :MonoBehaviour
{
    //new关键字隐藏基类的collider属性
    public new CapsuleCollider collider { get; protected set; }
    public float radius =>collider.radius;
    public Vector3 center =>transform.position;
    protected virtual void Awake()
    {
        tag = GameTags.Pole;
        collider=GetComponent<CapsuleCollider>();
    }

    public Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

    /// <summary>
    /// 某个Transform面向Pole的方向，并返回两者的距离
    /// </summary>
    /// <param name="other"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Vector3 GetDirectionToPole(Transform other, out float distance)
    {
        var target = new Vector3(center.x, other.position.y, center.z) - other.position;
        distance = target.magnitude;

        return target / distance;
    }

    public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
    {
        var minHeight = collider.bounds.min.y + offset;
        var maxHeight = collider.bounds.max.y + offset;
        var ClampedHeight =Mathf.Clamp(point.y, minHeight, maxHeight);
        return new Vector3(point.x, ClampedHeight,point.z);
    }
}
