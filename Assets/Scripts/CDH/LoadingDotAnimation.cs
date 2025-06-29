using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingDotAnimation : MonoBehaviour
{
    public TextMeshProUGUI loadingText; // 로딩중... 텍스트 UI 연결
    public float dotAnimationSpeed = 0.5f; // 점이 바뀌는 속도

    private void Start()
    {
        StartCoroutine(AnimateDots());
    }

    IEnumerator AnimateDots()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4; // 0,1,2,3 반복
            string dots = new string('.', dotCount);

            loadingText.text = $"로딩중{dots}";

            yield return new WaitForSeconds(dotAnimationSpeed);
        }
    }
}