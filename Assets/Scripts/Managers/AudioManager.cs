using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> LowIntensityMusic;
    public List<AudioSource> MidIntensityMusic;
    public List<AudioSource> HighIntensityMusic;

    public AudioSource currentlyPlaying;
    public WaitUntil waitForWhistle;
    public AudioSource tick;
    public AudioSource whistle;

    public static AudioManager instance;

    public void PlayBGM(GameplayLoop.INTENSITY intensity)
    {
        switch (intensity)
        {
            case GameplayLoop.INTENSITY.LOW:
                {
                    currentlyPlaying = LowIntensityMusic[Random.Range(0, LowIntensityMusic.Count)];
                    break;
                }
            case GameplayLoop.INTENSITY.MID:
                {
                    currentlyPlaying = MidIntensityMusic[Random.Range(0, MidIntensityMusic.Count)];
                    break;
                }
            case GameplayLoop.INTENSITY.HIGH:
            case GameplayLoop.INTENSITY.EXTREME:
                {
                    currentlyPlaying = HighIntensityMusic[Random.Range(0, HighIntensityMusic.Count)];
                    break;
                }
                
        }
        currentlyPlaying.volume = 1;
        currentlyPlaying.Play();
    }

    public void PlayTick()
    {
        tick.Play();
    }


    public void PlayWhistle()
    {
        whistle.Play();
    }

    public IEnumerator FadeOutTrack()
    {
        float volume = 1;
        while (volume > 0)
        {
            volume -= (Time.deltaTime * 0.25f);
            currentlyPlaying.volume = volume;
            yield return new WaitForFixedUpdate();
        }
    }

    private void Start()
    {
        instance = this;
        waitForWhistle = new WaitUntil(() => whistle.isPlaying == false); 
    }
}
