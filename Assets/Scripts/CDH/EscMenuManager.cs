using UnityEngine;

public class EscMenuManager : MonoBehaviour
{
    public GameObject escPanel;       // 1차 ESC 창
    public GameObject confirmPanel;   // 2차 "정말 종료?" 팝업

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject); // 씬 전환 시 유지
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // bool isOpening = !escPanel.activeSelf;

            escPanel.SetActive(!escPanel.activeSelf);
            //escPanel.SetActive(isOpening);
            confirmPanel.SetActive(false); // esc 누르면 확인창은 무조건 닫기

            // 게임 일시정지 or 재개
            //Time.timeScale = isOpening ? 0f : 1f;
        }
    }

    public void OnClickEscapeExit()
    {
        // "탈출하기" → 확인창 열기
        //escPanel.SetActive(false);
        confirmPanel.SetActive(true);
    }

    public void OnClickConfirmYes()
    {
        //        //Time.timeScale = 1f;
        //#if UNITY_EDITOR
        //        UnityEditor.EditorApplication.isPlaying = false;
        //#else
        //        Application.Quit();
        //#endif
        SceneLoad.LoadSceneWithLoading("UI_OP");
        confirmPanel.SetActive(false);
        escPanel.SetActive(false);
    }

    public void OnClickConfirmNo()
    {
        // 확인창 닫고 원래 ESC 메뉴로 돌아가기
        confirmPanel.SetActive(false);
        escPanel.SetActive(true);
    }

    public void OnClickBack()
    {

        // ESC 창 닫기
        escPanel.SetActive(false);
        //Time.timeScale = 1f;
    }

    public void ResetAllPrefs()
    {
#if UNITY_EDITOR
        PlayerPrefs.DeleteAll(); // 모든 키 삭제
        PlayerPrefs.Save(); // 저장
        Debug.Log("PlayerPrefs 초기화 완료!");
#endif
    }
}
