using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MapConnectionView : MonoBehaviour
{
    public MapConnectionDefinition connection;
    public Transform[] waypoints;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        DrawRoute();
        SetSelected(false);
    }

    public void DrawRoute()
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        line.positionCount = waypoints.Length;

        for (int i = 0; i < waypoints.Length; i++)
            line.SetPosition(i, waypoints[i].position);
    }

    public void SetSelected(bool selected)
    {
        if (line == null)
            line = GetComponent<LineRenderer>();

        Color c = selected ? selectedColor : normalColor;
        line.startColor = c;
        line.endColor = c;
    }
}
