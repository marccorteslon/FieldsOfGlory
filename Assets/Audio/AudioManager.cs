using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip worldMapMusic;
    public AudioClip townMusic;
    public AudioClip joustMusic;

    [Header("SFX Clips")]
    public AudioClip buttonClickSfx;
    public AudioClip buyItemSfx;
    public AudioClip travelSfx;
    public AudioClip lanceHitSfx;
    public AudioClip openPanelSfx;
    public AudioClip closePanelSfx;

    private AudioClip currentMusicClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
       

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null)
            return;

        if (currentMusicClip == clip)
            return;

        currentMusicClip = clip;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        currentMusicClip = null;
    }

    public void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayWorldMapMusic()
    {
        PlayMusic(worldMapMusic);
    }

    public void PlayTownMusic()
    {
        PlayMusic(townMusic);
    }

    public void PlayJoustMusic()
    {
        PlayMusic(joustMusic);
    }

    public void PlayButtonClick()
    {
        PlaySfx(buttonClickSfx);
    }

    public void PlayBuyItem()
    {
        PlaySfx(buyItemSfx);
    }

    public void PlayTravel()
    {
        PlaySfx(travelSfx);
    }

    public void PlayLanceHit()
    {
        PlaySfx(lanceHitSfx);
    }

    public void PlayOpenPanel()
    {
        PlaySfx(openPanelSfx);
    }

    public void PlayClosePanel()
    {
        PlaySfx(closePanelSfx);
    }
}