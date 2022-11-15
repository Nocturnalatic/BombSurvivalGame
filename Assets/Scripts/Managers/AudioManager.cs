using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public List<AudioSource> LobbyMusic;
    public List<AudioSource> LowIntensityMusic;
    public List<AudioSource> MidIntensityMusic;
    public List<AudioSource> HighIntensityMusic;
    public List<AudioSource> ExtIntensityMusic;

    public AudioSource currentlyPlaying;
    public WaitUntil waitForWhistle;
    public AudioSource tick;
    public AudioSource whistle;

    public static AudioManager instance;

    public void StopAudio()
    {
        currentlyPlaying.Stop();
    }

    public void PlayLobbyMusic()
    {
        int selector = Random.Range(0, LobbyMusic.Count);
        currentlyPlaying = LobbyMusic[selector];
        currentlyPlaying.volume = 1;
        currentlyPlaying.Play();
    }

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
                {
                    currentlyPlaying = HighIntensityMusic[Random.Range(0, HighIntensityMusic.Count)];
                    break;
                }
            case GameplayLoop.INTENSITY.EXTREME:
                {
                    currentlyPlaying = ExtIntensityMusic[Random.Range(0, ExtIntensityMusic.Count)];
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
