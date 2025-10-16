using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider),typeof(AudioSource))]
public class Breakable :MonoBehaviour
{
    public GameObject display;
    public AudioClip clip;
    public UnityEvent OnBreak;
    protected AudioSource m_audio;
    protected Collider m_collider;
    protected Rigidbody m_rigidBody;
    public bool broken { get; protected set; }

    public virtual void Break()
    {
        if (!broken)
        {
            m_rigidBody.isKinematic = true;
        }
        broken = true;
        display.SetActive(false);
        m_collider.enabled = false;
        m_audio.PlayOneShot(clip);
        OnBreak?.Invoke();
    }

    protected void Start()
    {
        m_audio = GetComponent<AudioSource>();
        m_collider = GetComponent<Collider>();
        TryGetComponent(out m_rigidBody);//可能没有
    }
}