using System.Collections;
using TMPro;
using UnityEngine;

public class Egg : MonoBehaviour
{
    private float countdownTime = 10f; // ī��Ʈ�ٿ� �ð�
    public TextMeshProUGUI countdownText;
    private bool isCountingDown = false; // ī��Ʈ�ٿ� ������ Ȯ��

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
            countdownText.text = "0"; // ī��Ʈ�ٿ� ���� ǥ��

        isCountingDown = false; // ī��Ʈ�ٿ� ���� �� �ٽ� ���� ����
    }
}
