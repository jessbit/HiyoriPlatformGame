using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽象基类，管理实体状态机，带事件支持
/// </summary>
public abstract class EntityStateManager :MonoBehaviour
{
    /// <summary>
    /// 状态管理相关事件集合
    /// </summary>
    public EntityStateManagerEvents events;
}

/// <summary>
/// 泛型抽象类，继承自EntityStateManager,管理特定实体类型T的状态机
/// </summary>
/// <typeparam name="T">实体类型，必须继承自Entity<T></typeparam>
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{
    protected List<EntityState<T>> m_list=new();
    protected Dictionary<Type,EntityState<T>> m_states=new();
    protected abstract List<EntityState<T>> GetStateList();
    public EntityState<T> current { get; protected set; }
    public EntityState<T> last { get; protected set; }
    public int index => m_list.IndexOf(current);
    public int lastIndex =>m_list.IndexOf(last);
    public T entity { get; protected set; }
    protected virtual void Start()
    {
        InitializeStates();
        InitializeEntity();
    }
    public virtual bool ContainsStateOfType(Type t)=>m_states.ContainsKey(t);
    protected virtual void InitializeEntity() => entity = GetComponent<T>();
    protected virtual void InitializeStates()
    {
        m_list = GetStateList();
        foreach (var state in m_list)
        {
            var type = state.GetType();
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }
        }

        if (m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    public virtual void Step()
    {
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }

    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);
        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    public virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }
    public virtual void Change(EntityState<T> to)
    {
        if (to != null && Time.timeScale > 0)
        {
            if (current != null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }
            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }

    public virtual void OnContact(Collider other)
    {
        if (current != null && Time.timeScale > 0)
        {
            current.OnContact(entity, other);
        }
    }
    public virtual bool IsCurrentOfType(Type type)
    {
        if (current == null)
        {
            return false;
        }

        return current.GetType() == type;
    }
}