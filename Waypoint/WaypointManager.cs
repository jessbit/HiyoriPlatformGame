using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointMode
{
    Loop,//循环:到达最后一个地点后从头开始循环
    PingPong,//往返模式
    Once
}
public class WaypointManager : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public WaypointMode mode; //pingpong,loop,once
    public float waitTime;
    public List<Transform> waypoints;
    protected Transform m_current;
    
    public int index =>waypoints.IndexOf(current);
    public Transform current
    {
        get
        {
            if (!m_current)
            {
                m_current = waypoints[0];
            }
            return m_current;
        }
        protected set
        {
            m_current = value;
        }
    }

    protected bool m_pong;//pingpong模式下的方向标记
    protected bool m_changing;
    public virtual void Next()
    {
        if (m_changing) return;
        if (mode == WaypointMode.PingPong)
        {
            if (!m_pong)
            {
                m_pong = (index + 1 == waypoints.Count);
            }
            else
            {
                m_pong = (index - 1 >= 0);
            }

            var next = !m_pong ? index + 1 : index - 1;
            StartCoroutine(Change(next));
        }
        else if (mode == WaypointMode.Loop)
        {
            if (index + 1 < waypoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
            else
            {
                StartCoroutine(Change(0));
            }
        }
        else if (mode == WaypointMode.Once)
        {
            if (index + 1 < waypoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
        }
    }
    protected virtual IEnumerator Change(int to)
    {
        m_changing = true;
        yield return new WaitForSeconds(waitTime);
        current = waypoints[to];
        m_changing = false;
    }
}