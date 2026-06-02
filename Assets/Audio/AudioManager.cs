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
            musicSource.pitch = 1f;
            // Ignorar la pausa del AudioListener para que el audio de música
            // no se vea afectado por Time.timeScale = 0 (pausa de juego)
            musicSource.ignoreListenerPause = true;
            // 2D puro: evita que la distancia al AudioManager afecte al volumen/pitch
            musicSource.spatialBlend = 0f;
            musicSource.dopplerLevel = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.pitch = 1f;
            sfxSource.ignoreListenerPause = true;
            sfxSource.spatialBlend = 0f;
            sfxSource.dopplerLevel = 0f;
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