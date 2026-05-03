using UnityEngine;
using System.Collections;

public class WorldEventPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Canvas group containing the Text and Buttons")]
    public CanvasGroup contentCanvasGroup; 
    public float fadeInDuration = 0.5f;

    private void OnEnable()
    {
        // Whenever this panel is turned on, instantly hide the text and buttons.
        // The sprite animation will start playing automatically.
        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.alpha = 0f;
            contentCanvasGroup.interactable = false;
            contentCanvasGroup.blocksRaycasts = false;
        }
    }

    // We will call this method from the Animation timeline!
    public void ShowContent()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeInRoutine());
        }
    }

    private IEnumerator FadeInRoutine()
    {
        float timeElapsed = 0f;
        while (timeElapsed < fadeInDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            
            if (contentCanvasGroup != null)
            {
                contentCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timeElapsed / fadeInDuration);
            }
            
            yield return null;
        }

        // Make sure it's fully visible and clickable at the end
        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.alpha = 1f;
            contentCanvasGroup.interactable = true;
            contentCanvasGroup.blocksRaycasts = true;
        }
    }
}
