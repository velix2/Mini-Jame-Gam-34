using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    [FormerlySerializedAs("audioSources")] [SerializeField] private AudioClip[] audioClips;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    public void PlayAudio(int id)
    {
        PlayAudio(audioClips[id]);
    }
    
    private void PlayAudio(AudioClip clip)
    {
        _audioSource.PlayOneShot(clip);
    }
    
}
