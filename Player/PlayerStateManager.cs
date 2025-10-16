using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerStateManager : EntityStateManager<Player>
{ 
    /// <summary>
    /// 玩家状态类的字符数组
    /// 使用ClassTypeName特性，让Unity Inspector面板中可下拉/输入选择对应状态类
    /// </summary>
    [ClassTypeName(typeof(PlayerState))]
    public string[] states;

    protected override List<EntityState<Player>> GetStateList()
    {
        return PlayerState.CreateListFromStringArray(states);
    }
}