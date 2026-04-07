using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialStartButton : MonoBehaviour
{
    [Header("Scene Settings")]
    public string tutorialSceneName = "JoustScene";

    public void StartTutorialJoust()
    {
        PlayerPrefs.SetInt("JoustTutorialEnabled", 1);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(tutorialSceneName);
    }
}