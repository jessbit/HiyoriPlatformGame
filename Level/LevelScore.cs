using UnityEngine;
using UnityEngine.Events;

public class LevelScore :Singleton<LevelScore>
{
    public UnityEvent<int> OnCoinsSet;
    public UnityEvent<bool[]> OnStarsSet;
    public UnityEvent OnScoreLoaded;
    
    public int coins
    {
        get { return m_coins; }

        set
        {
            m_coins = value;
            OnCoinsSet?.Invoke(m_coins);
        }
    }
    
    public float time { get; protected set; }
    public bool stopTime { get; set; } = true;

    protected int m_coins;
    protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];
    public bool[] stars => (bool[])m_stars.Clone();
    protected Game m_game;
    protected GameLevel m_level;
    protected virtual void Start()
    {
        m_game = Game.Instance;
        m_level = m_game?.GetCurrentLevel();

        if (m_level != null)
        {
            m_stars = (bool[])m_level.stars.Clone();
        }

        OnScoreLoaded?.Invoke();
    }
    /// <summary>
    /// 合并关卡数据
    /// </summary>
    public virtual void Consolidate()
    {
        if (m_level != null)
        {
            if (m_level.time == 0 || time < m_level.time)
            {
                m_level.time = time;
            }

            if (coins > m_level.coins)
            {
                m_level.coins = coins;
            }

            m_level.stars = (bool[])stars.Clone();
            m_game.RequestSaving();
        }
    }
    protected virtual void Update()
    {
        if (!stopTime)
        {
            time += Time.deltaTime;
        }
    }
    public virtual void CollectStar(int index)
    {
        m_stars[index] = true;
        OnStarsSet?.Invoke(m_stars);
    }

}