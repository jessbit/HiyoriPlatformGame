using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelStarter : Singleton<LevelStarter>
{
    public UnityEvent OnStart;
    public float enablePlayerDelay = 1f;
    protected LevelPauser m_pauser => LevelPauser.Instance;
    protected Level m_level=>Level.Instance;
    protected LevelScore m_score => LevelScore.Instance;
    protected virtual void Start()
    {
        StartCoroutine(Routine());
    }
    protected virtual IEnumerator Routine()
    {
        Game.LockCursor();
        m_level.player.controller.enabled = false;
        m_level.player.inputs.enabled = false;
        yield return new WaitForSeconds(enablePlayerDelay);
        OnStart?.Invoke();
        m_score.stopTime = false;
        m_level.player.controller.enabled = true;
        m_level.player.inputs.enabled = true;
        m_pauser.canPause = true;
    }

}