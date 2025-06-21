using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WhiteRoomCommandPuzzle : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip rightClip;      // 오른쪽(D) 효과음
    public AudioClip leftClip;       // 왼쪽(A) 효과음
    public AudioClip stayClip;       // 머무르기(W) 효과음
    public AudioClip clearClip;      // 클리어 효과음
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

        // 디버그 로그
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
            if (curr < doors.Count - 1) valid.Add("Right");
            if (curr > 0) valid.Add("Left");
            valid.Add("Stay");

            string cmd = valid[Random.Range(0, valid.Count)];
            sequence.Add((cmd, i));

            if (cmd == "Right") curr++;
            else if (cmd == "Left") curr--;
        }
    }

    private IEnumerator PuzzleRoutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < sequence.Count; i++)
        {
            var (cmd, zone) = sequence[i];

            // 1) 효과음 재생
            AudioClip clip = cmd == "Right" ? rightClip
                            : cmd == "Left" ? leftClip
                                             : stayClip;
            if (clip != null) audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds((clip?.length ?? 0f) + 0.2f);

            // 2) 입력 대기 및 판정
            bool success = false;
            bool inputReceived = false;
            KeyCode pressed = KeyCode.None;
            float timer = 0f;
            float timeout = inputDelay + gracePeriod;

            while (timer < timeout)
            {
                // 첫 GetKeyDown 이벤트만 잡기
                if (Input.GetKeyDown(KeyCode.D))
                {
                    inputReceived = true;
                    pressed = KeyCode.D;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    inputReceived = true;
                    pressed = KeyCode.A;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    inputReceived = true;
                    pressed = KeyCode.W;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // Stay 는 “아무 키도 안 눌림” 상태를 허용
            if (!inputReceived && cmd == "Stay")
            {
                success = true;
            }
            else if (inputReceived)
            {
                switch (cmd)
                {
                    case "Right": success = (pressed == KeyCode.D); break;
                    case "Left": success = (pressed == KeyCode.A); break;
                    case "Stay": success = (pressed == KeyCode.W); break;
                }
            }

            Debug.Log($"[{i + 1}] 명령: {cmd} → {(success ? "성공" : "실패")}");

            if (!success)
            {
                yield return StartCoroutine(FailureRoutine());
                yield break;
            }

            // 3) 문 열기 & 도착 대기
            doors[zone].Open();
            yield return new WaitUntil(() =>
                Vector3.Distance(player.position, doors[zone].transform.position)
                <= arrivalThreshold
            );
            yield return new WaitForSeconds(0.3f);
        }

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
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            audioSource.PlayOneShot(clearClip);
            Instantiate(rewardPrefab, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("보상 프리팹 또는 스폰 위치가 설정되지 않음.");
        }
        PuzzleStarted = true;
    }
}
