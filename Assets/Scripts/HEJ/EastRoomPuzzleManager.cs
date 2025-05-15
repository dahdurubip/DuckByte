using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EastLampPuzzleManager : MonoBehaviour
{
    [Header("Lamp Settings")]
    public List<GameObject> lamps;          // 5개 Lamp 오브젝트
    public float showTimePerLamp = 2f;      // 개별 표시 시간
    public float delayBeforeStart = 1f;     // 전체 깜빡임 후 대기

    [Header("Audio Settings")]
    public AudioSource audioSource;         // 효과음 재생용
    public AudioClip startClip;             // 퍼즐 시작 소리
    public AudioClip blinkClip;             // 램프 켜질 때마다 재생
    public AudioClip successClip;           // 성공 소리
    public AudioClip failClip;              // 실패 소리

    [Header("Success Spawn")]
    public GameObject successPrefab;        // 성공 시 생성할 오브젝트
    public Transform successSpawnPoint;     // 생성 위치

    private List<GameObject> sequence = new List<GameObject>();
    private int currentIndex = 0;
    private bool puzzleActive = false;

    public void StartPuzzle()
    {
        Debug.Log("PuzzleManager: StartPuzzle called");

        StopAllCoroutines();
        sequence.Clear();
        currentIndex = 0;
        puzzleActive = false;

        // 시작 사운드
        if (audioSource != null && startClip != null)
            audioSource.PlayOneShot(startClip);

        StartCoroutine(BlinkAllThenSequence());
    }

    private IEnumerator BlinkAllThenSequence()
    {
        // 전체 램프 켜기
        Debug.Log("PuzzleManager: BlinkAll ON");
        foreach (var lamp in lamps)
        {
            var ps = lamp.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
                // 블링크 사운드
                if (audioSource != null && blinkClip != null)
                    audioSource.PlayOneShot(blinkClip);
            }
        }

        yield return new WaitForSeconds(showTimePerLamp);

        // 전체 램프 끄기
        Debug.Log("PuzzleManager: BlinkAll OFF");
        foreach (var lamp in lamps)
        {
            var ps = lamp.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        yield return new WaitForSeconds(delayBeforeStart);

        // 시퀀스 표시
        yield return StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        sequence.Clear();
        List<GameObject> pool = new List<GameObject>(lamps);
        while (pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            sequence.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        Debug.Log("PuzzleManager: sequence generated");

        foreach (var lamp in sequence)
        {
            Debug.Log("PuzzleManager: lighting " + lamp.name);
            var ps = lamp.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Clear();
                ps.Play();
                // 블링크 사운드
                if (audioSource != null && blinkClip != null)
                    audioSource.PlayOneShot(blinkClip);
            }

            yield return new WaitForSeconds(showTimePerLamp);

            if (ps != null)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Debug.Log("PuzzleManager: waiting for player input");
        puzzleActive = true;
        currentIndex = 0;
    }

    public void TryLamp(GameObject lamp)
    {
        if (!puzzleActive) return;

        if (lamp == sequence[currentIndex])
        {
            currentIndex++;
            Debug.Log("PuzzleManager: correct lamp #" + currentIndex);

            if (currentIndex >= sequence.Count)
            {
                // 성공 처리
                puzzleActive = false;
                Debug.Log("PuzzleManager: puzzle success");

                if (audioSource != null && successClip != null)
                    audioSource.PlayOneShot(successClip);

                if (successPrefab != null && successSpawnPoint != null)
                {
                    var go = Instantiate(successPrefab,
                                         successSpawnPoint.position,
                                         successSpawnPoint.rotation);
                    go.SetActive(true);  // 인스턴스 활성화 확실히
                }
            }
        }
        else
        {
            // 실패 처리
            Debug.Log("PuzzleManager: wrong lamp, restarting");
            if (audioSource != null && failClip != null)
                audioSource.PlayOneShot(failClip);
            StartPuzzle();
        }
    }
}
