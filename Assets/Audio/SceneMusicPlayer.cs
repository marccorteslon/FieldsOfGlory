using UnityEngine;

public class SceneMusicPlayer : MonoBehaviour
{
    public enum MusicType
    {
        None,
        WorldMap,
        Town,
        Joust
    }

    public MusicType musicType = MusicType.None;

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