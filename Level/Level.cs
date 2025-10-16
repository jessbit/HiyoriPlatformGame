using UnityEngine;

public class Level :Singleton<Level>
{
    /// <summary>
    /// player的缓存引用,
    /// 第一次访问查找，之后使用缓存，避免重复调用Find方法
    /// </summary>
    protected Player m_player;

    public Player player
    {
        get
        {
            if (!m_player)
            {
                m_player = FindObjectOfType<Player>();
            }
            return m_player;
        }
    }
}