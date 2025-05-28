using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WhiteRoomCommandPuzzle : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip rightClip;      // 오른쪽(D/→) 효과음
    public AudioClip leftClip;       // 왼쪽(A/←) 효과음
    public AudioClip stayClip;
    public AudioSource audioSource;

    [Header("Player Reset")]
    public Transform player;
    public Transform playerStartPosition;

    [Header("VFX")]
    public GameObject failFog;

    [Header("Doors")]
    public List<DoorController> doors;

    [Header("Reward")]
    public GameObject rewardPrefab;
    public Transform rewardSpawnPoint;

    [Header("Timing")]
    public float inputDelay = 2.0f;
    public float gracePeriod = 0.5f;
    public float arrivalThreshold = 2.0f;

    public bool PuzzleStarted { get; private set; } = false;

    private List<(string command, int zone)> sequence = new List<(string, int)>();
    private int puzzleStartZone = -1;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (failFog != null) failFog.SetActive(false);
        if (doors == null || doors.Count == 0)
            Debug.LogWarning("Doors list is empty or not assigned.");
    }

    public void BeginPuzzle(int zoneIndex)
    {
        if (PuzzleStarted) return;
        if (doors == null || zoneIndex < 0 || zoneIndex >= doors.Count)
        {
            Debug.LogError("BeginPuzzle: invalid zoneIndex or doors not set.");
            return;
        }

        PuzzleStarted = true;
        puzzleStartZone = zoneIndex;
        GenerateSequence();

        // 디버그: sequence의 zone과 실제 doors[zone] 이름을 보여줌
        string seqLog = "";
        for (int i = 0; i < sequence.Count; i++)
        {
            seqLog += $"({sequence[i].command}, {sequence[i].zone}, {doors[sequence[i].zone]?.gameObject.name}) ";
        }
        Debug.Log($"BeginPuzzle: startZone={puzzleStartZone}, seq=[{seqLog}]");
        StartCoroutine(PuzzleRoutine());
    }

    private void GenerateSequence()
    {
        sequence.Clear();
        int curr = puzzleStartZone;
        for (int i = 0; i < doors.Count; i++)
        {
            var valid = new List<string>();
            if (curr < doors.Count - 1) valid.Add("Right");    // → or D
            if (curr > 0) valid.Add("Left");     // ← or A
            valid.Add("Stay");

            string cmd = valid[Random.Range(0, valid.Count)];
            sequence.Add((cmd, i)); // 반드시 i! (zone=0이면 Door1, zone=1이면 Door2...)

            if (cmd == "Right") curr++;
            else if (cmd == "Left") curr--;
            // Stay는 curr 그대로
        }
    }

    private IEnumerator PuzzleRoutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            var (cmd, zone) = sequence[i];
            Debug.Log($"[Step {i + 1}/{sequence.Count}] Command={cmd}, zone={zone}, doors[{zone}]={doors[zone]?.gameObject.name}");

            // 효과음 (방향에 맞게)
            AudioClip clip = cmd == "Right" ? rightClip
                            : cmd == "Left" ? leftClip
                                             : stayClip;
            if (clip != null) audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds((clip?.length ?? 0f) + 0.3f);

            // 입력 초기화
            yield return null; yield return null;
            Input.ResetInputAxes();
            yield return null;

            bool success = false;
            float startT = Time.time, limit = inputDelay + gracePeriod;

            while (Time.time - startT < limit)
            {
                if (cmd == "Right" &&
                    (Input.GetKeyDown(KeyCode.D) || Input.GetKey(KeyCode.D)))
                {
                    Debug.Log("[Input] Right OK");
                    success = true;
                    break;
                }
                if (cmd == "Left" &&
                    (Input.GetKeyDown(KeyCode.A) || Input.GetKey(KeyCode.A)))
                {
                    Debug.Log("[Input] Left OK");
                    success = true;
                    break;
                }
                if (cmd == "Stay" && Input.anyKeyDown)
                {
                    Debug.Log("[Input] Stay 실패: 키 입력됨");
                    success = false;
                    break;
                }
                yield return null;
            }
            if (cmd == "Stay" && !success)
                success = true;

            Debug.Log($"Input check for {cmd}: success={success}");
            if (!success)
            {
                yield return StartCoroutine(FailureRoutine());
                yield break;
            }

            // 모든 명령이든 맞으면 무조건 문 열기!
            Debug.Log($"[Door] Opening index={zone}, name={doors[zone].gameObject.name}");
            doors[zone].Open();

            yield return new WaitUntil(() =>
                Vector3.Distance(player.position, doors[zone].transform.position)
                <= arrivalThreshold
            );
            yield return new WaitForSeconds(0.5f);
        }

        // 최종 성공: 아이템 스폰, 퍼즐 재시작 불가
        PuzzleComplete();
    }


    private IEnumerator FailureRoutine()
    {
        foreach (var d in doors) d.ResetDoor();

        if (player != null && playerStartPosition != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = playerStartPosition.position;
            player.rotation = playerStartPosition.rotation;
            if (cc != null) cc.enabled = true;
        }

        if (failFog != null)
        {
            failFog.SetActive(true);
            failFog.GetComponent<VisualEffect>()?.Play();
        }
        yield return new WaitForSeconds(2f);
        if (failFog != null) failFog.SetActive(false);

        PuzzleStarted = false;
    }

    private void PuzzleComplete()
    {
        Debug.Log("Puzzle Complete! Spawning reward.");

        if (rewardPrefab != null && rewardSpawnPoint != null)
            Instantiate(rewardPrefab,
                        rewardSpawnPoint.position,
                        rewardSpawnPoint.rotation);
        else
            Debug.LogWarning("RewardPrefab or RewardSpawnPoint not assigned.");

        PuzzleStarted = true;
    }
}
