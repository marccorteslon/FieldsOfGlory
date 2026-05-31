using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialStartButton : MonoBehaviour
{
    [Header("Scene Settings")]
    public string tutorialSceneName = "NewTutorial";

    public void StartTutorialJoust()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(tutorialSceneName);
    }
}