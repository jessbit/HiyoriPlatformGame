﻿using UnityEngine;
[RequireComponent(typeof(Enemy))]
public class EnemyAudio:MonoBehaviour
{
    [Header("Effects")]
    public AudioClip death;

    protected Enemy m_enemy;
    protected AudioSource m_audio;
    protected virtual void InitializeEnemy() => m_enemy = GetComponent<Enemy>();

    protected virtual void InitializeAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }
    protected virtual void InitializeCallbacks()
    {
        m_enemy.enemyEvents.OnDie.AddListener(() => m_audio.PlayOneShot(death));
    }
}