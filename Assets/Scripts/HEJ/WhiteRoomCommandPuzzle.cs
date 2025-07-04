// WhiteRoomCommandPuzzle.cs
using UnityEngine;
using TMPro;
using UnityEngine.VFX;
using System.Collections;
using System.Collections.Generic;

public class WhiteRoomCommandPuzzle : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip rightClip;
    public AudioClip leftClip;
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
    public float inputDelay = 1.5f;
    public float gracePeriod = 0.2f;

    [Header("UI")]
    public TextMeshProUGUI lastInputDisplay;

    public bool PuzzleStarted { get; private set; } = false;

    private List<(string command, int zone)> sequence = new List<(string, int)>();
    private int puzzleStartZone = -1;
    private bool zoneReached = false;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (failFog != null)
            failFog.SetActive(false);

        if (lastInputDisplay != null)
            lastInputDisplay.gameObject.SetActive(false);
    }

    // ZoneTrigger에서 호출
    public void NotifyZoneReached(int zoneIndex)
    {
        if (!PuzzleStarted)
            BeginPuzzle(zoneIndex);
        else
            zoneReached = true;
    }

    private void BeginPuzzle(int zoneIndex)
    {
        PuzzleStarted = true;
        puzzleStartZone = zoneIndex;
        GenerateSequence();
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
            valid.Add("Forward");

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

            AudioClip clip = cmd == "Right" ? rightClip
                            : cmd == "Left" ? leftClip
                                             : stayClip;
            if (clip != null)
                audioSource.PlayOneShot(clip);

            float waitUntil = Time.time + (clip?.length ?? 0f) + inputDelay;
            KeyCode pressedKey = KeyCode.None;
            string displayText = string.Empty;

            while (Time.time < waitUntil)
            {
                if (Input.GetKeyDown(KeyCode.D))
                {
                    pressedKey = KeyCode.D;
                    displayText = "오른쪽";
                    break;
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    pressedKey = KeyCode.A;
                    displayText = "왼쪽";
                    break;
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    pressedKey = KeyCode.W;
                    displayText = "전진";
                    break;
                }
                yield return null;
            }

            if (pressedKey == KeyCode.None)
                displayText = "입력 없음";

            bool success = (pressedKey == KeyCode.None)
                        ? (cmd == "Forward")
                        : ((cmd == "Right" && pressedKey == KeyCode.D)
                           || (cmd == "Left" && pressedKey == KeyCode.A)
                           || (cmd == "Forward" && pressedKey == KeyCode.W));

            string styled = success ? $"<b>{displayText}</b>" : $"<u>{displayText}</u>";
            lastInputDisplay.text = styled;
            lastInputDisplay.gameObject.SetActive(true);

            yield return new WaitForSeconds(gracePeriod);

            if (!success)
            {
                yield return StartCoroutine(FailureRoutine());
                yield break;
            }

            doors[zone].Open();

            if (i < sequence.Count - 1)
            {
                zoneReached = false;
                yield return new WaitUntil(() => zoneReached);
                yield return new WaitForSeconds(0.3f);
            }

            lastInputDisplay.gameObject.SetActive(false);
        }

        // 퍼즐 클리어
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            audioSource.PlayOneShot(clearClip);
            Instantiate(rewardPrefab, rewardSpawnPoint.position, rewardSpawnPoint.rotation);
        }
        lastInputDisplay.gameObject.SetActive(false);
    }

    private IEnumerator FailureRoutine()
    {
        lastInputDisplay?.gameObject.SetActive(false);
        doors.ForEach(d => d.ResetDoor());

        var cc = player.GetComponent<CharacterController>();
        cc.enabled = false;
        player.SetPositionAndRotation(playerStartPosition.position, playerStartPosition.rotation);
        cc.enabled = true;

        failFog?.SetActive(true);
        failFog?.GetComponent<VisualEffect>()?.Play();
        yield return new WaitForSeconds(2f);
        failFog?.SetActive(false);

        PuzzleStarted = false;

        // 모든 ZoneTrigger를 찾아 ResetTrigger 호출
        var zoneTriggers = FindObjectsOfType<ZoneTrigger>();
        foreach (var z in zoneTriggers)
            z.ResetTrigger();
    }
}
