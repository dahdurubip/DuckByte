using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class Creature1TimelineManager : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public Camera mainCamera;
    public Camera cutsceneCamera;

    void OnEnable()
    {
        // playableDirector가 할당되었는지 확인하고 stopped 이벤트에 메소드를 등록합니다.
        if (playableDirector != null)
        {
            playableDirector.stopped += OnCutsceneFinished;
        }
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다.
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnCutsceneFinished;
        }
    }

    // 컷씬이 종료되었을 때 호출될 메소드
    void OnCutsceneFinished(PlayableDirector director)
    {
        // 컷씬 카메라를 비활성화하고 메인 카메라를 활성화합니다.
        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(false);
        }
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }
        // 여기에 추가적인 컷씬 종료 로직을 넣을 수 있습니다. (예: 플레이어 조작 활성화)
        Debug.Log("컷씬 종료! 메인 카메라로 전환합니다.");
    }
}
