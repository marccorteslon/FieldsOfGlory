using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAudio : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler,
    ISelectHandler,
    ISubmitHandler
{
    [Header("Audio")]
    public AudioClip hoverClip;
    public AudioClip clickClip;

    public bool useHoverSound = true;
    public bool useClickSound = true;

    private bool hasPlayedHover = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayHover();
    }

   
    public void OnSelect(BaseEventData eventData)
    {
        PlayHover();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        PlayClick();
    }

    
    public void OnSubmit(BaseEventData eventData)
    {
        PlayClick();
    }

    void PlayHover()
    {
        if (!useHoverSound || hasPlayedHover)
            return;

        hasPlayedHover = true;

        if (AudioManager.Instance == null)
            return;

        if (hoverClip != null)
            AudioManager.Instance.PlaySfx(hoverClip);
        else
            AudioManager.Instance.PlayButtonClick();
    }

    void PlayClick()
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