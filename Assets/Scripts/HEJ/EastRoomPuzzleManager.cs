using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EastRoomPuzzleManager : MonoBehaviour
{
    public static EastRoomPuzzleManager Instance { get; private set; }

    [Header("Assign in Inspector")]
    public LightProp[] lamps;

    [Header("Reward")]
    public GameObject rewardPrefab;       // 스폰할 아이템 프리팹
    public Transform rewardSpawnPoint;   // 스폰될 위치(Transform)

    private List<int> sequence;
    private int currentIndex = 0;
    private Transform player;

    void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;

        var pgo = GameObject.FindGameObjectWithTag("Player");
        if (pgo != null) player = pgo.transform;
    }

    void Update()
    {
        if (player == null || lamps == null || lamps.Length == 0)
            return;

        // 1) 플레이어와 가까운 램프 하나 찾기
        LightProp closest = null;
        float minDist = float.MaxValue;
        foreach (var lamp in lamps)
        {
            if (!lamp.IsInRange(player))
                continue;

            float d = Vector3.Distance(player.position, lamp.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = lamp;
            }
        }

        // 2) 모든 램프 아이콘 끄고, closest만 켬
        foreach (var lamp in lamps)
            lamp.ShowIcon(lamp == closest);

        // 3) E키 입력 시 closest 하나만 처리
        if (closest != null && Input.GetKeyDown(KeyCode.E))
            OnLampPressed(closest);
    }

    public void StartPuzzle()
    {
        StopAllCoroutines();
        currentIndex = 0;
        StartCoroutine(InitialFlash());
    }

    IEnumerator InitialFlash()
    {
        // 모두 켜졌다
        foreach (var lamp in lamps) lamp.SetState(true);
        yield return new WaitForSeconds(1.0f);
        // 모두 꺼졌다
        foreach (var lamp in lamps) lamp.SetState(false);
        yield return new WaitForSeconds(0.5f);

        // 시퀀스 생성 및 재생
        GenerateSequence();
        yield return StartCoroutine(ShowSequence());
    }

    void GenerateSequence()
    {
        sequence = new List<int>();
        for (int i = 0; i < lamps.Length; i++)
            sequence.Add(i);

        for (int i = 0; i < sequence.Count - 1; i++)
        {
            int r = Random.Range(i, sequence.Count);
            int tmp = sequence[i];
            sequence[i] = sequence[r];
            sequence[r] = tmp;
        }
        currentIndex = 0;
    }

    IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (int id in sequence)
        {
            lamps[id].SetState(true);
            yield return new WaitForSeconds(1.0f);
            lamps[id].SetState(false);
            yield return new WaitForSeconds(0.5f);
        }
    }

    void OnLampPressed(LightProp lamp)
    {
        if (lamp.lampID != sequence[currentIndex])
        {
            StartCoroutine(FailureRoutine());
            return;
        }

        lamp.SetState(true);
        currentIndex++;
        if (currentIndex >= sequence.Count)
            PuzzleComplete();
    }

    IEnumerator FailureRoutine()
    {
        foreach (var l in lamps) l.SetState(true);
        yield return new WaitForSeconds(1.0f);
        foreach (var l in lamps) l.SetState(false);
        yield return new WaitForSeconds(0.5f);
        StartPuzzle();
    }

 

    private void PuzzleComplete()
    {
       // Debug.Log("Puzzle Complete!");

        // 1) Reward Spawn
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            Instantiate(rewardPrefab,
                        rewardSpawnPoint.position,
                        rewardSpawnPoint.rotation);
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
        }
        else
        {
           // Debug.LogWarning("Reward Prefab or SpawnPoint not set!");
        }

    }
}
