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

    private List<AudioSource> LowIntMusicRealTime;
    private List<AudioSource> MidIntMusicRealTime;
    private List<AudioSource> HighIntMusicRealTime;
    private List<AudioSource> ExtIntMusicRealTime;

    public AudioSource currentlyPlaying;
    public WaitUntil waitForWhistle;
    public AudioSource tick;
    public AudioSource whistle;
    public AudioSource eventTrigger;

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
        AudioSource chosen;
        switch (intensity)
        {
            case GameplayLoop.INTENSITY.LOW:
                {
                    if (LowIntMusicRealTime.Count <= 0)
                    {
                        LowIntMusicRealTime = new List<AudioSource>(LowIntensityMusic);
                    }
                    chosen = LowIntMusicRealTime[Random.Range(0, LowIntMusicRealTime.Count)];
                    currentlyPlaying = chosen;
                    LowIntMusicRealTime.Remove(chosen);
                    break;
                }
            case GameplayLoop.INTENSITY.MID:
                {
                    if (MidIntMusicRealTime.Count <= 0)
                    {
                        MidIntMusicRealTime = new List<AudioSource>(MidIntensityMusic);
                    }
                    chosen = MidIntMusicRealTime[Random.Range(0, MidIntMusicRealTime.Count)];
                    currentlyPlaying = chosen;
                    MidIntMusicRealTime.Remove(chosen);
                    break;
                }
            case GameplayLoop.INTENSITY.HIGH:
                {
                    if (HighIntMusicRealTime.Count <= 0)
                    {
                        HighIntMusicRealTime = new List<AudioSource>(HighIntensityMusic);
                    }
                    chosen = HighIntMusicRealTime[Random.Range(0, HighIntMusicRealTime.Count)];
                    currentlyPlaying = chosen;
                    HighIntMusicRealTime.Remove(chosen);
                    break;
                }
            case GameplayLoop.INTENSITY.EXTREME:
            case GameplayLoop.INTENSITY.GLITCH:
            case GameplayLoop.INTENSITY.CRASH:
                {
                    if (ExtIntMusicRealTime.Count <= 0)
                    {
                        ExtIntMusicRealTime = new List<AudioSource>(ExtIntensityMusic);
                    }
                    chosen = ExtIntMusicRealTime[Random.Range(0, ExtIntMusicRealTime.Count)];
                    currentlyPlaying = chosen;
                    ExtIntMusicRealTime.Remove(chosen);
                    break;
                }

        }
        currentlyPlaying.volume = 0.75f;
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
        float volume = currentlyPlaying.volume;
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
        LowIntMusicRealTime = new List<AudioSource>(LowIntensityMusic);
        MidIntMusicRealTime = new List<AudioSource>(MidIntensityMusic);
        HighIntMusicRealTime = new List<AudioSource>(HighIntensityMusic);
        ExtIntMusicRealTime = new List<AudioSource>(ExtIntensityMusic);
    }
}
