using UnityEngine;
using UnityEngine.UI;

public class SensitivitySettingsController : MonoBehaviour
{
    public Slider sensitivitySlider;

    void Awake()
    {
        // Try to automatically find the slider by name if it is not assigned
        if (sensitivitySlider == null) sensitivitySlider = FindSliderByName("SensitivitySlider");
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
        float sensitivityVal = PlayerPrefs.GetFloat("MouseSensitivityMultiplier", 1.0f);

        if (sensitivitySlider != null)
        {
            // Map the multiplier to the slider value.
            // If the slider is 0 to 1, map 0.1 - 2.0 multiplier to 0 - 1 slider value
            if (sensitivitySlider.minValue == 0f && sensitivitySlider.maxValue == 1f)
            {
                sensitivitySlider.value = (sensitivityVal - 0.1f) / 1.9f;
            }
            else
            {
                sensitivitySlider.value = sensitivityVal;
            }

            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
    }

    private void OnSensitivityChanged(float val)
    {
        float actualMultiplier = val;
        if (sensitivitySlider.minValue == 0f && sensitivitySlider.maxValue == 1f)
        {
            actualMultiplier = 0.1f + (val * 1.9f);
        }
        PlayerPrefs.SetFloat("MouseSensitivityMultiplier", actualMultiplier);
        PlayerPrefs.Save();
    }
}
