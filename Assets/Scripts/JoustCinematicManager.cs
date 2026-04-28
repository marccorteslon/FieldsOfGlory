using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class JoustCinematicManager : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera firstPersonCam;
    public CinemachineCamera attackSlowCam;
    public CinemachineCamera enemyImpactCam;
    public CinemachineCamera playerCelebrateCam;

    //Cinematica inicial

    public CinemachineCamera OverviewCam;
    public CinemachineCamera walkingPlayerCam;

    [Header("Timing")]
    public float enemyImpactDuration = 3f;
    public float celebrateDuration = 3f;

    [Header("Time Scale")]
    public float attackSlowTimeScale = 0.35f;
    public float enemyImpactTimeScale = 0.25f;

    private float defaultFixedDeltaTime;

    void Awake()
    {
        defaultFixedDeltaTime = Time.fixedDeltaTime;
        ResetTimeScale();
        SetCamera(firstPersonCam);
    }

    public void SetCamera(CinemachineCamera activeCam)
    {
        SetPriority(firstPersonCam, activeCam);
        SetPriority(attackSlowCam, activeCam);
        SetPriority(enemyImpactCam, activeCam);
        SetPriority(playerCelebrateCam, activeCam);
    }

    void SetPriority(CinemachineCamera cam, CinemachineCamera activeCam)
    {
        if (cam == null) return;
        cam.Priority = cam == activeCam ? 100 : 0;
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * scale;
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }

    public void StartHorsePhaseCamera()
    {
        ResetTimeScale();
        SetCamera(firstPersonCam);
    }

    public void StartAttackPhaseCamera()
    {
        SetCamera(attackSlowCam);
        SetTimeScale(attackSlowTimeScale);
    }

    public void OnAttackInputReleased()
    {
        ResetTimeScale();
    }

    public IEnumerator PlayEnemyImpactSequence(bool playCelebrateAfter)
    {
        // cambio a cámara impacto con tiempo normal
        ResetTimeScale();
        SetCamera(enemyImpactCam);

        // pequeña pausa real para que el cambio de cámara respire
        yield return new WaitForSecondsRealtime(0.1f);

        // cámara lenta del enemigo volando
        SetTimeScale(enemyImpactTimeScale);

        yield return new WaitForSecondsRealtime(enemyImpactDuration);

        ResetTimeScale();

        if (playCelebrateAfter)
        {
            SetCamera(playerCelebrateCam);
            yield return new WaitForSecondsRealtime(celebrateDuration);
        }
        else
        {
            SetCamera(firstPersonCam);
        }
    }

    void OnDestroy()
    {
        ResetTimeScale();
    }
}