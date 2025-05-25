using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSource.clip = audioClip;

        audioSource.volume = volume;

        audioSource.Play();

        float clipLength = audioClip.length;

        Destroy(audioSource.gameObject, clipLength + 0.1f); // Add a small buffer to ensure the sound finishes playing before destroying
    }
    public void StopAllSoundFX()
    {
        foreach (var audioSource in FindObjectsOfType<AudioSource>())
        {
            if (audioSource != null && audioSource != soundFXObject && audioSource.isPlaying)
            {
                audioSource.Stop();
                Destroy(audioSource.gameObject);
            }
        }
    }
    public void StopSoundFX(AudioClip audioClip)
    {
        foreach (var audioSource in FindObjectsOfType<AudioSource>())
        {
            if (audioSource != null && audioSource != soundFXObject && audioSource.isPlaying && audioSource.clip == audioClip)
            {
                audioSource.Stop();
                Destroy(audioSource.gameObject);
            }
        }
    }
    public AudioSource PlayLoopingSoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.Play();
        return audioSource;
    }
    



}
