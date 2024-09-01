using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyDeathAudioPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _playbackStarted;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        _audioSource.Play();
        _playbackStarted = true;
    }

    private void Update()
    {
        if (!_audioSource.isPlaying && _playbackStarted)
        {
            Destroy(gameObject);
        }
    }
}
