// WhiteRoomCommandPuzzle.cs
using UnityEngine;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WhiteRoomCommandPuzzle : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip forwardClip;
    public AudioClip backwardClip;
    public AudioClip stayClip;
    public AudioSource audioSource;

    [Header("Player Reset")]
    public Transform player;
    public Transform playerStartPosition;

    [Header("VFX")]
    public GameObject failFog;

    [Header("Doors")]
    public List<DoorController> doors;

    [Header("Timing")]
    public float inputDelay = 2.0f;
    public float gracePeriod = 0.5f;
    public float arrivalThreshold = 2.0f;

    public bool PuzzleStarted { get; private set; } = false;

    // 내부 시퀀스 저장
    private int puzzleZoneIndex = -1;
    private List<(string command, int zone)> sequence = new List<(string, int)>();

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (failFog != null)
            failFog.SetActive(false);

        if (doors == null || doors.Count == 0)
            Debug.LogWarning("WhiteRoomCommandPuzzle: doors list is empty or not assigned.");
    }

    // ZoneTrigger에서 호출
    public void BeginPuzzle(int zoneIndex)
    {
        if (PuzzleStarted)
            return;

        if (doors == null || zoneIndex < 0 || zoneIndex >= doors.Count)
        {
            Debug.LogError("BeginPuzzle: invalid zoneIndex or doors not set.");
            return;
        }

        PuzzleStarted = true;
        puzzleZoneIndex = zoneIndex;
        sequence.Clear();

        string[] cmds = { "Forward", "Backward", "Stay" };
        for (int i = 0; i < 3; i++)
            sequence.Add((cmds[Random.Range(0, cmds.Length)], zoneIndex));

        Debug.Log($"BeginPuzzle: zone={zoneIndex}, seq=[{string.Join(",", sequence)}]");
        StartCoroutine(PuzzleRoutine());
    }

    private IEnumerator PuzzleRoutine()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("PuzzleRoutine: start");

        // 키 매핑
        KeyCode forwardKey1 = KeyCode.W;
        KeyCode forwardKey2 = KeyCode.UpArrow;
        KeyCode backwardKey1 = KeyCode.S;
        KeyCode backwardKey2 = KeyCode.DownArrow;

        for (int i = 0; i < sequence.Count; i++)
        {
            var entry = sequence[i];
            string cmd = entry.command;  // Item1
            int zone = entry.zone;     // Item2
            Debug.Log($"Command {i + 1}/{sequence.Count}: {cmd}");

            // 1) 음성 재생
            AudioClip clip = cmd == "Forward" ? forwardClip
                            : cmd == "Backward" ? backwardClip
                            : stayClip;
            if (clip != null)
                audioSource.PlayOneShot(clip);
            yield return new WaitForSeconds((clip?.length ?? 0f) + 0.3f);

            // 2) 입력 초기화
            yield return null;
            yield return null;
            Input.ResetInputAxes();
            Debug.Log("Input flushed");

            // 3) 입력 판정
            bool success = false;
            float timer = 0f;
            float limit = inputDelay + gracePeriod;

            if (cmd == "Stay")
            {
                // Stay: 지정 시간 동안 W/S 또는 화살표 키 입력이 없으면 성공
                while (timer < limit)
                {
                    if (Input.GetKeyDown(forwardKey1) || Input.GetKeyDown(forwardKey2) ||
                        Input.GetKeyDown(backwardKey1) || Input.GetKeyDown(backwardKey2))
                        break;
                    timer += Time.deltaTime;
                    yield return null;
                }
                success = timer >= limit;
            }
            else if (cmd == "Forward")
            {
                while (timer < limit)
                {
                    if (Input.GetKeyDown(forwardKey1) || Input.GetKeyDown(forwardKey2))
                    {
                        Debug.Log("[Input] Forward detected");
                        success = true;
                        break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else if (cmd == "Backward")
            {
                while (timer < limit)
                {
                    if (Input.GetKeyDown(backwardKey1) || Input.GetKeyDown(backwardKey2))
                    {
                        Debug.Log("[Input] Backward detected");
                        success = true;
                        break;
                    }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            Debug.Log($"Input check for {cmd}: success={success}");
            if (!success)
            {
                yield return StartCoroutine(FailureRoutine());
                yield break;
            }

            // 4) 문 열기
            Debug.Log($"[Debug] Opening door at index={zone}, name={doors[zone].gameObject.name}");
            doors[zone].Open();

            // 5) 플레이어가 문 앞에 도착할 때까지 대기
            Vector3 doorPos = doors[zone].transform.position;
            while (Vector3.Distance(player.position, doorPos) > arrivalThreshold)
                yield return null;

            yield return new WaitForSeconds(0.5f);
        }

        // 6) 퍼즐 완전 성공
        PuzzleComplete();
    }

    private IEnumerator FailureRoutine()
    {
        Debug.Log("FailureRoutine: start");

        // a) 모든 문 닫기
        foreach (var d in doors)
            d.ResetDoor();

        // b) 플레이어 즉시 시작 위치로 리셋
        if (player != null && playerStartPosition != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = playerStartPosition.position;
            player.rotation = playerStartPosition.rotation;
            if (cc != null) cc.enabled = true;

            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // c) 연기 이펙트 재생
        if (failFog != null)
        {
            failFog.SetActive(true);
            var vfx = failFog.GetComponent<VisualEffect>();
            if (vfx != null) vfx.Play();
        }

        // d) 대기
        yield return new WaitForSeconds(2f);

        // e) 연기 끄기
        if (failFog != null)
            failFog.SetActive(false);

        // f) 퍼즐 상태 리셋
        PuzzleStarted = false;
        Debug.Log("FailureRoutine: end");
    }

    private void PuzzleComplete()
    {
        Debug.Log("Puzzle Complete!");
        PuzzleStarted = false;
        // 성공 보상 로직 추가 가능
    }
}
