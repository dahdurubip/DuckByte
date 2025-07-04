using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Button startButton;      // "게임 시작" 버튼 (처음 실행 시)
    [SerializeField] private Button continueButton;   // "이어하기" 버튼
    [SerializeField] private Button newGameButton;    // "새 게임" 버튼
    [SerializeField] private GameObject realNewGame;    // "새 게임" 버튼

    void Start()
    {
        bool hasSave = PlayerPrefs.HasKey("SavedScene");

        if (hasSave)
        {
            // 저장된 값이 있으면 이어하기/새 게임 보여주고 "게임 시작" 버튼은 숨김
            startButton.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(true);
            newGameButton.gameObject.SetActive(true);
        }
        else
        {
            // 저장값 없으면 "게임 시작" 버튼만 보여줌
            startButton.gameObject.SetActive(true);
            continueButton.gameObject.SetActive(false);
            newGameButton.gameObject.SetActive(false);
        }
    }


    //public void GameStart()
    //{
    //    //SceneManager.LoadScene("OP");
    //    //SceneLoad.LoadSceneWithLoading("OP");
    //    // 저장된 씬 이름 불러오기 (PlayerPrefs)
    //    string sceneToLoad = PlayerPrefs.GetString("SavedScene", "OP");
    //    SceneLoad.LoadSceneWithLoading(sceneToLoad); // 저장된 씬으로 로드
    //}
    
    // 처음 시작
    public void OnClickStart()
    {
        SceneLoad.LoadSceneWithLoading("OP");
    }

    // 이어하기
    public void OnClickContinue()
    {
        string sceneToLoad = PlayerPrefs.GetString("SavedScene", "OP");
        SceneLoad.LoadSceneWithLoading(sceneToLoad);
    }

    // 새 게임
    public void OnClickNewGame()
    {
        realNewGame.SetActive(true);

    }
    public void OnClickRealNewGameNo()
    {
        realNewGame.SetActive(false);

    }

    public void OnClickRealNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        SceneLoad.LoadSceneWithLoading("OP");
    }

    public void GameEnd()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
