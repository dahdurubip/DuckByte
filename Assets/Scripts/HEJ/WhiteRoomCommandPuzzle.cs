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
    public AudioClip clearClip;
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

            // 1) 효과음 재생
            AudioClip clip = cmd == "Right" ? rightClip
                            : cmd == "Left" ? leftClip
                                             : stayClip;
            if (clip != null) audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds((clip?.length ?? 0f) + 0.2f);

            // 2) **키 리셋** (잡힌 키가 없도록)
            yield return null;
            while (Input.anyKey) yield return null;

            // 3) **입력 판정**
            bool success = false;
            float timer = 0f;
            float timeout = inputDelay + (cmd == "Stay" ? 0f : gracePeriod);

            while (timer < timeout)
            {
                // 오른쪽 판정 (D 키 또는 → 화살표)
                if (cmd == "Right" &&
                   (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    success = true;
                    break;
                }

                // 왼쪽 판정 (A 키 또는 ← 화살표)
                if (cmd == "Left" &&
                   (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)))
                {
                    success = true;
                    break;
                }

                // 머무르기(Stay)는 **절대** A/D/←/→ 가 눌리지 않아야 성공
                if (cmd == "Stay" &&
                   (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                    Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    success = false;
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // Stay 커맨드는 timeout까지 아무 키도 눌리지 않았다면 성공
            if (cmd == "Stay" && timer >= inputDelay)
                success = true;

            Debug.Log($"[{i + 1}] {cmd} 판정 → {(success ? "OK" : "Fail")}");

            if (!success)
            {
                yield return StartCoroutine(FailureRoutine());
                yield break;
            }

            // 4) 문 열기 & 플레이어 도착 대기
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

        if (rewardPrefab != null && rewardSpawnPoint != null) { 
        
            audioSource.PlayOneShot(clearClip);
            Instantiate(rewardPrefab,
                     rewardSpawnPoint.position,
                     rewardSpawnPoint.rotation);

        }
     
        else
            Debug.Log("not");
        PuzzleStarted = true;
    }
}
