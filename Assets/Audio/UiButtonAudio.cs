using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAudio : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Audio")]
    public bool useHoverSound = true;
    public bool useClickSound = true;

    public AudioClip hoverClip;
    public AudioClip clickClip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!useHoverSound)
            return;

        if (AudioManager.Instance == null)
            return;

        if (hoverClip != null)
            AudioManager.Instance.PlaySfx(hoverClip);
        else
            AudioManager.Instance.PlayButtonClick(); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!useClickSound)
            return;

        if (AudioManager.Instance == null)
            return;

        if (clickClip != null)
            AudioManager.Instance.PlaySfx(clickClip);
        else
            AudioManager.Instance.PlayButtonClick(); 
    }
}