using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController _instance;
    public static AudioController Instance { get { return _instance; } }

    public AudioSource audioSource;
    public AudioSource loopSource;

    [Header("Player SFX")]
    public AudioClip jumpSound;
    public AudioClip flutterSound;
    public AudioClip[] footstepSounds; 

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
    }

    public void PlayJump(float pitch = 1f)
    {
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(jumpSound);
    }

    public void StartFlutterLoop()
    {

        if (loopSource.clip == flutterSound && loopSource.isPlaying) return;

        loopSource.clip = flutterSound;
        loopSource.loop = true;
        loopSource.Play();
        
    }

    public void StopFlutterLoop()
    {
        // Only stop the audio if it is currently playing the flutter sound
        if (loopSource.clip == flutterSound && loopSource.isPlaying)
        {
            loopSource.Stop();
            loopSource.clip = null;
        }
    }

    public void PlayFootstep()
    {
        if (footstepSounds.Length == 0) return;

        // Choose random footstep sound so its somewhat unique and modify pitch a bit each time
        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.pitch = Random.Range(1.7f, 1.9f);

        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }

}