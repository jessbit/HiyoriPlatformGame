using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 监听实体状态管理器状态切换事件的组件
/// 监听特定状态的进入和退出事件，触发对应的UnityEvent
/// </summary>
[AddComponentMenu("Game/Entity/Entity State Manager Listener")]
public class EntityStateManagerListener:MonoBehaviour
{
    //对应时间触发的对应事件
    public UnityEvent onEnter;
    public UnityEvent onExit;
    //需要监听的状态列表
    public List<string> states;
    protected EntityStateManager m_manager;

    protected virtual void Start()
    {
        if (!m_manager)
        {
            m_manager = GetComponentInParent<EntityStateManager>();
            m_manager.events.onEnter.AddListener(OnEnter);
            m_manager.events.onExit.AddListener(OnExit);
        }
    }

    protected virtual void OnEnter(Type state)
    {
        if (states.Contains(state.Name))
        {
            onEnter?.Invoke();
        }
    }

    protected virtual void OnExit(Type state)
    {
        if (states.Contains(state.Name))
        {
            onExit?.Invoke();
        }
    }
}
