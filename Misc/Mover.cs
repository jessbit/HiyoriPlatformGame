using System.Collections;
using UnityEngine;
public class Mover:MonoBehaviour
{
    public Vector3 offset;
    public float duration;
    public float resetDuration;

    protected Vector3 m_initialPosition;
    /// <summary>
    /// 平滑移动
    /// </summary>
    public virtual void ApplyOffset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(m_initialPosition, m_initialPosition + offset, duration));
    }

    public virtual void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(transform.localPosition, m_initialPosition, resetDuration));
    }
    
    /// <summary>
    /// 协程处理平滑
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
    {
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            var t = elapsedTime / duration;
            transform.localPosition = Vector3.Lerp(from, to, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = to;
    }

    protected virtual void Start()
    {
        m_initialPosition = transform.localPosition;
    }
}