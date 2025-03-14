using System.Collections;
using TMPro;
using UnityEngine;

public class Egg : MonoBehaviour
{
    private float countdownTime = 10f; // 카운트다운 시간
    public TextMeshProUGUI countdownText;
    private bool isCountingDown = false; // 카운트다운 중인지 확인

    //private void OnCollisionEnter(Collision col)
    //{
    //    if (col.gameObject.CompareTag("Player") && !isCountingDown)
    //    {
    //        StartCoroutine(StartCountdown());
    //    }
    //}

    private void OnTriggerEnter(Collider juju)
    {
        if (juju.gameObject.CompareTag("Player") && !isCountingDown)
        {
            StartCoroutine(StartCountdown());
        }
    }

    IEnumerator StartCountdown()
    {
        isCountingDown = true;
        float timeLeft = countdownTime;

        while (timeLeft > 0)
        {
            if (countdownText != null)
                countdownText.text = timeLeft.ToString("F0");

            yield return new WaitForSeconds(1f);
            timeLeft--;
        }

        if (countdownText != null)
            countdownText.text = "0"; // 카운트다운 종료 표시

        isCountingDown = false; // 카운트다운 종료 후 다시 감지 가능
    }
}
