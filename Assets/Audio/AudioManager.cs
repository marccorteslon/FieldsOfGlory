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

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SfxVolume => sfxVolume;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Load settings from PlayerPrefs
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);

        AudioListener.volume = masterVolume;

        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.pitch = 1f;
            musicSource.volume = musicVolume;
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
            sfxSource.volume = sfxVolume;
            sfxSource.ignoreListenerPause = true;
            sfxSource.spatialBlend = 0f;
            sfxSource.dopplerLevel = 0f;
        }
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
        AudioListener.volume = masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SfxVolume", sfxVolume);
        PlayerPrefs.Save();
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
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
        musicSource.volume = musicVolume;
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

        sfxSource.PlayOneShot(clip, sfxVolume);
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