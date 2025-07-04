using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GoToBoss : MonoBehaviour
{
    public PlayableDirector timeline; // 인스펙터에서 타임라인 추가

    void Start()
    {
        if (timeline != null)
        {
            timeline.stopped += OnTimelineEnd; // 타임라인 종료 시 이벤트 실행
        }
    }

    void OnTimelineEnd(PlayableDirector director)
    {
        if (director == timeline)
        {
            GameSaveManager.SaveProgress("Boss 1");
            SceneLoad.LoadSceneWithLoading("Boss 1");
        }
    }
}