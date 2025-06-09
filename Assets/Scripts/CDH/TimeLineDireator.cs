using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;
//using Unity.Cinemachine;

public class TimeLineDireator: MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector pd;

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera mainCam;      // 기본 플레이 카메라
    public CinemachineVirtualCamera timelineCam;  // 타임라인에 사용된 V-Cam

    void OnEnable()
    {
        pd.stopped += OnTimelineStopped;
    }

    void OnDisable()
    {
        pd.stopped -= OnTimelineStopped;
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        // 타임라인 카메라 비활성화
        timelineCam.Priority = 0;
        // 기본 카메라 우선순위 높여서 전환
        mainCam.Priority = 10;
    }
}
