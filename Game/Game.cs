using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Game : Singleton<Game>
{
    public UnityEvent<int> OnRetriesSet;
    protected int m_retries;
    public int retries
    {
        get { return m_retries; }

        set
        {
            m_retries = value;
            OnRetriesSet?.Invoke(m_retries);
        }
    }
    public UnityEvent OnSavingRequested;

    public int initialRetries = 3;
    public List<GameLevel> levels;
    protected int m_dataIndex;
    protected DateTime m_createdAt;
    protected DateTime m_updatedAt;
    
    public static void LockCursor(bool value = true)
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        Cursor.visible = !value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
    }
    public virtual void RequestSaving()
    {
        GameSaver.Instance.Save(ToData(), m_dataIndex);
        OnSavingRequested?.Invoke();
    }
    public virtual GameLevel GetCurrentLevel()
    {
        var scene = GameLoader.Instance.currentScene;
        return levels.Find((level) => level.scene == scene);
    }
    public virtual int GetCurrentLevelIndex()
    {
        var scene = GameLoader.Instance.currentScene;
        return levels.FindIndex((level) => level.scene == scene);
    }
    public virtual void LoadState(int index, GameData data)
    {
        m_dataIndex = index;
        m_retries = data.retries;
        m_createdAt = DateTime.Parse(data.createdAt);
        m_updatedAt = DateTime.Parse(data.updatedAt);

        for (int i = 0; i < data.levels.Length; i++)
        {
            levels[i].LoadState(data.levels[i]);
        }
    }
    public virtual void UnlockNextLevel()
    {
        var index = GetCurrentLevelIndex() + 1;

        if (index >= 0 && index < levels.Count)
        {
            levels[index].locked = false;
        }
    }
    public virtual LevelData[] LevelsData()
    {
        return levels.Select(level => level.ToData()).ToArray();
    }
    public virtual GameData ToData()
    {
        return new GameData()
        {
            retries = m_retries,
            levels =  LevelsData(),
            createdAt = m_createdAt.ToString(),
            updatedAt = DateTime.UtcNow.ToString()
        };
    }

    protected override void Awake()
    {
        base.Awake();
        retries = initialRetries;
        DontDestroyOnLoad(gameObject);
    }
}