using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public Vector2 offset = new Vector2(10f, -10f);

    private RectTransform panelRect;
    private Canvas canvas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panelRect = tooltipPanel.GetComponent<RectTransform>();
        canvas = tooltipPanel.GetComponentInParent<Canvas>();
        tooltipPanel.SetActive(false);
    }

    void Update()
    {
        if (tooltipPanel.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );

            Vector2 anchoredPos = pos + offset;

            // Limitar al canvas
            Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;
            Vector2 panelSize = panelRect.sizeDelta;

            anchoredPos.x = Mathf.Clamp(anchoredPos.x, -canvasSize.x / 2 + panelSize.x / 2, canvasSize.x / 2 - panelSize.x / 2);
            anchoredPos.y = Mathf.Clamp(anchoredPos.y, -canvasSize.y / 2 + panelSize.y / 2, canvasSize.y / 2 - panelSize.y / 2);

            panelRect.localPosition = anchoredPos;
        }
    }

    public void ShowTooltip(string content)
    {
        tooltipText.text = content;
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}