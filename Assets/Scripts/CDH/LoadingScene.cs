using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingScene : MonoBehaviour
{
    public Slider progressBar; // Inspector에서 Slider 연결

    private float targetProgress = 0;

    void Start()
    {
        StartCoroutine(DelayThenLoad());
    }

    IEnumerator DelayThenLoad()
    {
        yield return null; // 한 프레임 대기

        StartCoroutine(LoadNextScene());
    }


    IEnumerator LoadNextScene()
    {
        string nextScene = PlayerPrefs.GetString("NextScene"); // 저장된 씬 이름 가져오기

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene); // 비동기 로드 시작
        op.allowSceneActivation = false; // 로드 완료 후 자동 전환 X

        while (!op.isDone)
        {
            // 진행도를 0~1 사이로 계산
            float rawProgress = Mathf.Clamp01(op.progress / 0.9f);
            targetProgress = rawProgress;

            progressBar.value = Mathf.Lerp(progressBar.value, targetProgress, Time.deltaTime * 2.5f);


            if (op.progress >= 0.9f && progressBar.value >= 0.98f)
            {
                progressBar.value = 1f;
                // 잠깐 딜레이를 주거나 로딩 완료 UI 표시 가능
                yield return new WaitForSeconds(0.5f);
                op.allowSceneActivation = true; // 이제 씬 전환 허용
            }

            yield return null;
        }
    }
}
