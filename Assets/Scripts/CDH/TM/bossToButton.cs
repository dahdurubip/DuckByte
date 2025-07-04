using UnityEngine;
using UnityEngine.Playables;

public class bossToButton : MonoBehaviour
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
            //SceneManager.LoadScene("Creature2Map"); // 전환할 씬 이름 입력
            // 보스클리어 저장
            GameSaveManager.SaveProgress("button");
            SceneLoad.LoadSceneWithLoading("button");

        }
    }
}
