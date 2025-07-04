using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class OpeningToCreature1 : MonoBehaviour
{
    public PlayableDirector timeline; // 인스펙터에서 타임라인 추가

    void Start()
    {
        
        
            //Debug.Log("시작: SavedScene = " + PlayerPrefs.GetString("SavedScene", "없음"));
        


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
            GameSaveManager.SaveProgress("Creature2Map");
            SceneLoad.LoadSceneWithLoading("Creature2Map");

        }
    }
}