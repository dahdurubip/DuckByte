using UnityEngine;
using System;
using System.Collections;

public class SpiderEgg : MonoBehaviour
{
    private bool isCounting = false;
    private bool isPlayerInRange = false;
    private Coroutine countdownCoroutine;
    private EJPlayer currentPlayer;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} 거미알이 플레이어를 감지!");

            currentPlayer = other.GetComponent<EJPlayer>();
            if (currentPlayer != null)
            {
                currentPlayer.SetEscapeCallback(OnPlayerEscape); // X 버튼 탈출 기능 활성화
                currentPlayer.ResetXPressCount(); // X 입력 횟수 초기화
            }

            isPlayerInRange = true;

            // 카운트다운이 시작되지 않았다면 시작
            if (!isCounting)
            {
                isCounting = true;
                countdownCoroutine = StartCoroutine(Countdown());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && currentPlayer != null)
        {
            Debug.Log($"{gameObject.name} 플레이어가 범위를 벗어남! X 버튼이 비활성화됩니다.");
            currentPlayer.SetEscapeCallback(null); // X 버튼 기능 비활성화
            currentPlayer = null;
            isPlayerInRange = false;
        }
    }

    IEnumerator Countdown()
    {
        float timer = 10f;
        while (timer > 0)
        {
            Debug.Log($"{gameObject.name} 남은 시간: {timer:F1}초");
            timer -= 1f;
            yield return new WaitForSeconds(1f);
        }

        Debug.Log($"{gameObject.name} 시간 초과! 플레이어가 실패했습니다.");
        isCounting = false;
    }

    private void OnPlayerEscape()
    {
        Debug.Log($"{gameObject.name} 플레이어가 탈출을 성공했습니다!");

        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        Destroy(gameObject);
    }
}
