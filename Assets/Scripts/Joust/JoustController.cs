using System.Threading;
using UnityEngine;

public class JoustController : MonoBehaviour
{
    [SerializeField] Transform joustTransform;
    [SerializeField] float mouseSensibility;

    float mousePreviousX, mousePreviousY;

    void Start()
    {
        mousePreviousX = Input.mousePosition.x;
        mousePreviousY = Input.mousePosition.y;
    }

    private void OnApplicationFocus(bool focus)
    {
        mousePreviousX = Input.mousePosition.x;
        mousePreviousY = Input.mousePosition.y;
    }

    void Update()
    {
        Vector2 input = ReadInput();
        RotateTransform(input);
    }

    Vector2 ReadInput()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;

        Vector2 result = new Vector2(mouseX - mousePreviousX, mouseY - mousePreviousY) * mouseSensibility;

        mousePreviousX = mouseX;
        mousePreviousY = mouseY;

        return result;
    }

    void RotateTransform(Vector2 rotation)
    {
/*        rotation.x = Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad)) * 1.0f;
        rotation.y = 0.0f;
        Debug.Log("rotation = " + rotation.x + ", " + rotation.y);
        joustTransform.Rotate(new Vector3(-rotation.y, rotation.x, 0.0f), Space.World);
*/
        joustTransform.localEulerAngles += new Vector3(-rotation.y, rotation.x, 0.0f);
    }
}
