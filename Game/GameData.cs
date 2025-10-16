using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 游戏数据类，用于保存游戏存档信息
/// </summary>
[Serializable]
public class GameData
{
    public int retries;
    public LevelData[] levels;
    public string createdAt;
    public string updatedAt;
    public static GameData Create()
    {
        return new GameData()
        {
            retries = Game.Instance.initialRetries,
            createdAt = DateTime.UtcNow.ToString(),
            updatedAt = DateTime.UtcNow.ToString(),
            levels = Game.Instance.levels.Select((level) =>
            {
                return new LevelData()
                {
                    locked = level.locked
                };
            }).ToArray()
        };
    }

    /// <summary>
    /// 返回关卡星星总和
    /// </summary>
    public virtual int TotalStars()
    {
        return levels.Aggregate(0, (acc, level) =>
        {
            var total = level.CollectedStars();
            return acc + total;
        });
    }

    /// <summary>
    /// 返回所有关卡星星总和
    /// </summary>
    /// <returns></returns>
    public virtual int TotalCoins()
    {
        return levels.Aggregate(0, (acc, level) => acc + level.coins);
    }

    public virtual string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public static GameData FromJson(string json)
    {
        return JsonUtility.FromJson<GameData>(json);
    }
}