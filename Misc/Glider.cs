using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : MonoBehaviour
{
    public Player player;
    protected AudioSource m_audio;
    public TrailRenderer[] trails;
    public float scaleDuration = 0.7f;

    [Header("Audio Settings")] public AudioClip openAudio;
    public AudioClip closeAudio;

    protected virtual void Start()
    {
        InitializePlayer();
        InitializeAudio();
        InitializeCallbacks();
        InitializeGlider();
    }
    protected virtual void InitializePlayer()
    {
        if (!player) player = GetComponentInParent<Player>();
    }

    protected virtual void InitializeAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }

    protected virtual void InitializeCallbacks()
    {
        player.playerEvents.OnGlidingStart.AddListener(ShowGlider);
        player.playerEvents.OnGlidingStop.AddListener(HideGlider);
    }

    /// <summary>
    /// 初始化滑翔翼，隐藏(缩放为0，拖尾关闭)
    /// </summary>
    protected virtual void InitializeGlider()
    {
        SetTrailsEmitting(false);
        transform.localPosition = Vector3.zero;
    }
    
    protected virtual void ShowGlider()
    {
        //停止正在运行的缩放动画，避免冲突
        StopAllCoroutines();
        StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));
        SetTrailsEmitting(true);
        m_audio.PlayOneShot(openAudio);
    }

    protected virtual void HideGlider()
    {
        StopAllCoroutines();
        StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero));
        SetTrailsEmitting(false);
        m_audio.PlayOneShot(closeAudio);
    }

    protected virtual void SetTrailsEmitting(bool state)
    {
        if(trails == null) return;

        foreach (var trail in  trails)
        {
            trail.emitting = state;
        }
    }
    protected virtual IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
    {
        var time = 0f;
        transform.localScale = from;

        while (time < scaleDuration)
        {
            var scale = Vector3.Lerp(from, to, time / scaleDuration);
            transform.localScale = scale;
            time += Time.deltaTime;
            yield return null;
        }
        //最后确保完全缩放到目标值
        transform.localScale = to;
    }
}