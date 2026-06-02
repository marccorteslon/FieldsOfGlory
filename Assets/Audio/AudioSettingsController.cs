using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsController : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void Awake()
    {
        // Try to automatically find the sliders by name if they are not assigned
        if (masterSlider == null) masterSlider = FindSliderByName("MasterSlider");
        if (musicSlider == null) musicSlider = FindSliderByName("MusicSlider");
        if (sfxSlider == null) sfxSlider = FindSliderByName("SFXSlider");
    }

    private Slider FindSliderByName(string name)
    {
        // Look in children first
        foreach (Slider s in GetComponentsInChildren<Slider>(true))
        {
            if (s.gameObject.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return s;
            }
        }
        // Look in active scene
        foreach (Slider s in FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (s.gameObject.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return s;
            }
        }
        return null;
    }

    void Start()
    {
        // Initialize slider values from PlayerPrefs or AudioManager
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SfxVolume", 1f);

        if (AudioManager.Instance != null)
        {
            masterVol = AudioManager.Instance.MasterVolume;
            musicVol = AudioManager.Instance.MusicVolume;
            sfxVol = AudioManager.Instance.SfxVolume;
        }

        if (masterSlider != null)
        {
            masterSlider.value = masterVol;
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
            sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(val);
        }
        else
        {
            PlayerPrefs.SetFloat("MasterVolume", val);
            PlayerPrefs.Save();
            AudioListener.volume = val;
        }
    }

    private void OnMusicVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(val);
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", val);
            PlayerPrefs.Save();
        }
    }

    private void OnSfxVolumeChanged(float val)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(val);
        }
        else
        {
            PlayerPrefs.SetFloat("SfxVolume", val);
            PlayerPrefs.Save();
        }
    }
}
