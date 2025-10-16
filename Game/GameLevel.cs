using System;
using UnityEngine;

/// <summary>
/// 游戏关卡的数据结构
/// 用于记录关卡的状态
/// 主要用于存储与读取关卡数据，格式化关卡时间显示
/// </summary>
[Serializable]
public class GameLevel
{
    public bool locked;
    public string scene;
    public string name;
    public string description;
    public Sprite image;
    
    public int coins { get; set; }
    public float time { get; set; }
    public static readonly int StarsPerLevel = 3;
    public bool[] stars { get; set; } = new bool[StarsPerLevel];
    
    public virtual LevelData ToData()
    {
        return new LevelData()
        {
            locked = this.locked,
            coins = this.coins,
            time = this.time,
            stars = this.stars
        };
    }
    public virtual void LoadState(LevelData data)
    {
        locked = data.locked;
        coins = data.coins;
        time = data.time;
        stars = data.stars;
    }
    /// <summary>
    /// 格式化时间
    /// 将给定的时间格式化为00'00''00
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string FormattedTime(float time)
    {
        var minutes = Mathf.FloorToInt(time / 60f);
        var seconds = Mathf.FloorToInt(time % 60f);
        var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
    }
}