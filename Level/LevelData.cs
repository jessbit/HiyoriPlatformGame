using System;
using System.Linq;

/// <summary>
/// 单个关卡的进度信息
/// </summary>
[Serializable]
public class LevelData
{
    public bool locked;
    public int coins;
    public float time;
    public bool[] stars = new bool[GameLevel.StarsPerLevel];

    /// <summary>
    /// 返回已经被收集的星星数量
    /// </summary>
    /// <returns></returns>
    public int CollectedStars()
    {
        return stars.Where((star) => star).Count();
    }
}