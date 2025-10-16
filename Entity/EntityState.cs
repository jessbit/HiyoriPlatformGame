using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[Serializable]
public abstract class EntityState<T> where T : Entity<T>
{
    public UnityEvent onEnter;
    public UnityEvent onExit;
    public float timeSinceEntered { get; protected set; }

    public void Enter(T entity)
    {
        timeSinceEntered = 0;
        onEnter?.Invoke();
        OnEnter(entity);
    }
    public void Exit(T entity)
    {
        onEnter?.Invoke();
        OnExit(entity);
    }

    public void Step(T entity)
    {
        OnStep(entity);
        timeSinceEntered+= Time.deltaTime;
    }
    protected abstract void OnEnter(T player);
    protected abstract void OnExit(T entity);
    protected abstract void OnStep(T entity);
    /// <summary>
    /// 实体与其他碰撞体接触时调用，用于处理碰撞逻辑
    /// </summary>
    /// <param name="entity">当前实体</param>
    /// <param name="other">碰撞到的其他碰撞体</param>
    public abstract void OnContact(T entity,Collider other);
    public static EntityState<T> CreateListFromString(string typeName)
    {
        return (EntityState<T>)System.Activator
            .CreateInstance(System.Type.GetType(typeName));
    }
    public static List<EntityState<T>> CreateListFromStringArray(string[] array)
    {
        var list= new List<EntityState<T>>();
        foreach (var typeName in array)
        {
            list.Add(CreateListFromString(typeName));
        }
        return list;
    }       
}
