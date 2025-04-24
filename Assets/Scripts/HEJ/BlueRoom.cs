// 동쪽 방
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueRoom : MonoBehaviour
{
    public GameObject[] lanterns;            // 석등 오브젝트
    public GameObject pavilion;              // 정자 (보상 위치)
    public GameObject rewardItem;            // 파편 오브젝트
    public float lightDuration = 2f;         // 석등 밝기 유지 시간
    public int maxAttempts = 3;

    private List<int> lightSequence = new List<int>();
    private int currentInput = 0;
    private int attemptsLeft;
    private bool inputEnabled = false;

    void Start()
    {
        rewardItem.SetActive(false);
        pavilion.SetActive(false);
        attemptsLeft = maxAttempts;
        StartCoroutine(PlayLightSequence());
    }

    IEnumerator PlayLightSequence()
    {
        yield return new WaitForSeconds(1f);
        lightSequence.Clear();

        List<int> indices = new List<int>();
        for (int i = 0; i < lanterns.Length; i++) indices.Add(i);
        for (int i = 0; i < lanterns.Length; i++)
        {
            int rnd = indices[Random.Range(0, indices.Count)];
            indices.Remove(rnd);
            lightSequence.Add(rnd);
        }

        foreach (int index in lightSequence)
        {
            SetLanternLight(index, true);
            yield return new WaitForSeconds(lightDuration);
            SetLanternLight(index, false);
            yield return new WaitForSeconds(0.5f);
        }

        inputEnabled = true;
        currentInput = 0;
    }

    public void SelectLantern(int index)
    {
        if (!inputEnabled) return;

        if (index == lightSequence[currentInput])
        {
            currentInput++;
            if (currentInput >= lightSequence.Count)
            {
                PuzzleComplete();
            }
        }
        else
        {
            attemptsLeft--;
            if (attemptsLeft > 0)
            {
                StartCoroutine(RetrySequence());
            }
            else
            {
                PuzzleFailed();
            }
        }
    }

    void SetLanternLight(int index, bool state)
    {
        Light lanternLight = lanterns[index].GetComponentInChildren<Light>();
        if (lanternLight != null)
        {
            lanternLight.enabled = state;
        }
    }

    IEnumerator RetrySequence()
    {
        inputEnabled = false;
        yield return new WaitForSeconds(1f);
        StartCoroutine(PlayLightSequence());
    }

    void PuzzleComplete()
    {
        inputEnabled = false;
        pavilion.SetActive(true);
        rewardItem.SetActive(true);
        Debug.Log("푸른 길 퍼즐 완성! 파편이 등장합니다.");
    }

    void PuzzleFailed()
    {
        inputEnabled = false;
        Debug.Log("푸른 안개에 길을 잃었습니다... 퍼즐 실패.");
        // 리셋하거나 보스 봉인 해제 처리 등 추가 가능
    }
}
