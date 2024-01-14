using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AUDIO_FILTER_EFFECTS
    {
        NONE = 0,
        LOW_HEALTH,
        UNDERWATER,
        TOTAL
    }

    #region BGMs
    public List<AudioSource> LobbyMusic;
    public List<AudioSource> LowIntensityMusic;
    public List<AudioSource> MidIntensityMusic;
    public List<AudioSource> HighIntensityMusic;
    public List<AudioSource> ExtIntensityMusic;
    public List<AudioSource> GchIntensityMusic;

    private List<AudioSource> LowIntMusicRealTime;
    private List<AudioSource> MidIntMusicRealTime;
    private List<AudioSource> HighIntMusicRealTime;
    private List<AudioSource> ExtIntMusicRealTime;
    private List<AudioSource> GchIntMusicRealTime;
    #endregion

    public AudioSource currentlyPlaying;
    public WaitUntil waitForWhistle;
    public AudioSource tick;
    public AudioSource whistle;
    public AudioSource eventTrigger;

    public static AudioManager instance;

    private Camera mainCamera;
    private AudioReverbFilter filter;

    public void StopAudio()
    {
        currentlyPlaying.Stop();
    }

    public void ApplyAudioFilter(AUDIO_FILTER_EFFECTS fx)
    {
        switch (fx)
        {
            case AUDIO_FILTER_EFFECTS.NONE:
                {
                    filter.reverbPreset = AudioReverbPreset.Off;
                    mainCamera.GetComponent<AudioLowPassFilter>().enabled = false;
                    break;
                }
            case AUDIO_FILTER_EFFECTS.LOW_HEALTH:
                {
                    filter.reverbPreset = AudioReverbPreset.Dizzy;
                    mainCamera.GetComponent<AudioLowPassFilter>().enabled = true;
                    break;
                }
            case AUDIO_FILTER_EFFECTS.UNDERWATER:
                {
                    filter.reverbPreset = AudioReverbPreset.Underwater;
                    mainCamera.GetComponent<AudioLowPassFilter>().enabled = true;
                    break;
                }
            default:
                filter.reverbPreset = AudioReverbPreset.Off;
                break;
        }
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
            case GameplayLoop.INTENSITY.GLITCH:
            case GameplayLoop.INTENSITY.CRASH:
                {
                    if (GchIntMusicRealTime.Count <= 0)
                    {
                        GchIntMusicRealTime = new List<AudioSource>(GchIntensityMusic);
                    }
                    chosen = GchIntMusicRealTime[Random.Range(0, GchIntMusicRealTime.Count)];
                    currentlyPlaying = chosen;
                    GchIntMusicRealTime.Remove(chosen);
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
        GchIntMusicRealTime = new List<AudioSource>(GchIntensityMusic);
        mainCamera = Camera.main;
        filter = mainCamera.GetComponent<AudioReverbFilter>();
    }
}
