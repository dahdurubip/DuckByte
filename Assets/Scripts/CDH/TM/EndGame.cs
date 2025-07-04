using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndGame : MonoBehaviour
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
            // 저장값 완전히 초기화
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            SceneLoad.LoadSceneWithLoading("UI_OP");

        }
    }
}