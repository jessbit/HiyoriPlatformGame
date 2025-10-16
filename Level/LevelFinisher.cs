using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelFinisher :Singleton<LevelFinisher>
{
    public UnityEvent OnFinish;


    public UnityEvent OnExit;

    public bool unlockNextLevel;
    public string nextScene;
    public string exitScene;
    public float loadingDelay = 1f;

    protected Game m_game => Game.Instance;
    protected Level m_level => Level.Instance;
    protected LevelScore m_score => LevelScore.Instance;
    protected LevelPauser m_pauser => LevelPauser.Instance;
    protected GameLoader m_loader => GameLoader.Instance;
    protected Fader m_fader => Fader.Instance;

    protected virtual IEnumerator FinishRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_score.stopTime = true;
        m_level.player.inputs.enabled = false;

        yield return new WaitForSeconds(loadingDelay);

        if (unlockNextLevel)
        {
            m_game.UnlockNextLevel();
        }

        m_score.Consolidate();
        m_loader.Load(nextScene);
        Game.LockCursor(false);
        OnFinish?.Invoke();
    }

    protected virtual IEnumerator ExitRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.inputs.enabled = false;
        yield return new WaitForSeconds(loadingDelay);
        m_loader.Load(exitScene);
        Game.LockCursor(false);
        OnExit?.Invoke();
    }
    
    public virtual void Finish()
    {
        StopAllCoroutines();
        StartCoroutine(FinishRoutine());
    }
    
    public virtual void Exit()
    {
        StopAllCoroutines();
        StartCoroutine(ExitRoutine());
    }
}