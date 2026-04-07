using UnityEngine;

public class SceneMusicStarter : MonoBehaviour
{
    public enum MusicType
    {
        WorldMap,
        Town,
        Joust
    }

    public MusicType musicType;

    void Start()
    {
        if (AudioManager.Instance == null)
            return;

        switch (musicType)
        {
            case MusicType.WorldMap:
                AudioManager.Instance.PlayWorldMapMusic();
                break;

            case MusicType.Town:
                AudioManager.Instance.PlayTownMusic();
                break;

            case MusicType.Joust:
                AudioManager.Instance.PlayJoustMusic();
                break;
        }
    }
}