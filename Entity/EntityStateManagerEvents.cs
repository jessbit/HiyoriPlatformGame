using System;
using UnityEngine.Events;

[Serializable]
public class EntityStateManagerEvents
{
    public UnityEvent onChange;
    /// <summary>
    /// 进入状态时触发的事件
    /// 传递被进入状态的类型信息，方便外部根据状态类型做不同处理
    /// </summary>
    public UnityEvent<Type> onEnter;
    /// <summary>
    /// 退出状态时触发的事件
    /// 传递被退出状态的类型信息，方便外部根据状态类型做不同处理
    /// </summary>
    public UnityEvent<Type> onExit;
}
