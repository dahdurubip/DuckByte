using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteRoomCommandPuzzle : MonoBehaviour
{
    public AudioClip forwardClip;
    public AudioClip backwardClip;
    public AudioClip stayClip;
    public AudioSource audioSource;

    public Transform player;
    public Transform playerStartPosition;

    public GameObject failFog;
    public List<DoorController> doors;

    public float inputDelay = 2.0f;
    public float gracePeriod = 0.5f;

    private List<(string command, int zone)> commandSequence = new();
    private int currentIndex = 0;
    private int lastSuccessZone = -1;
    public bool PuzzleStarted { get; private set; } = false;

    void Start()
    {
      
        failFog.SetActive(false);
    }

    public void BeginPuzzle()
    {
        if (PuzzleStarted) return;

        int zone = player.GetComponent<PlayerZoneState>().GetZone();
        Debug.Log("[퍼즐 시작] 현재 Zone Index: " + zone);

        if (zone < 0 || zone >= doors.Count) return;

        PuzzleStarted = true;
        GenerateCommandSequence(zone);
        StartCoroutine(StartPuzzle());
    }

    IEnumerator StartPuzzle()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < commandSequence.Count; i++)
        {
            string command = commandSequence[i].command;
            int zone = commandSequence[i].zone;

            while (player.GetComponent<PlayerZoneState>().GetZone() != zone)
            {
                yield return null;
            }

            player.GetComponent<PlayerZoneState>().inPuzzleMode = true;

            AudioClip clipToPlay = GetClipFromCommand(command);
            audioSource.PlayOneShot(clipToPlay);
            yield return new WaitForSeconds(clipToPlay.length + 0.3f);

            float timer = 0f;
            bool success = false;

            while (timer < inputDelay + gracePeriod)
            {
                if (CheckPlayerAction(commandSequence[i]))
                {
                    success = true;
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (!success)
            {
                PuzzleFail();
                yield break;
            }

            Debug.Log("[문 열림] 성공한 Zone Index: " + zone);

            if (zone >= 0 && zone < doors.Count)
            {
                doors[zone].Open();
                lastSuccessZone = zone;
                Debug.Log("[성공 기록] lastSuccessZone = " + lastSuccessZone);
            }

            yield return new WaitForSeconds(0.5f);
        }

        FinalSuccess();
    }

    void FinalSuccess()
    {
        Debug.Log("퍼즐 성공: 현재 구역 완료!");
       

        PuzzleStarted = false;
        currentIndex = 0;

        player.GetComponent<PlayerZoneState>().ExitPuzzleMode();
    }

    void GenerateCommandSequence(int zone)
    {
        commandSequence.Clear();

        string[] possibleCommands = { "Forward", "Backward", "Stay" };

        int numberOfQuestions = 3;

        for (int i = 0; i < numberOfQuestions; i++)
        {
            string randomCommand = possibleCommands[Random.Range(0, possibleCommands.Length)];
            commandSequence.Add((randomCommand, zone));
        }
    }

    void PlayCommandSound(string command)
    {
        if (command == "Forward")
            audioSource.PlayOneShot(forwardClip);
        else if (command == "Backward")
            audioSource.PlayOneShot(backwardClip);
        else if (command == "Stay")
            audioSource.PlayOneShot(stayClip);
    }

    bool CheckPlayerAction((string command, int zone) expected)
    {
        var zoneState = player.GetComponent<PlayerZoneState>();
        int currentZone = zoneState.GetZone();

        if (expected.command == "Forward" && currentZone == expected.zone)
            return Input.GetKeyDown(KeyCode.W);

        if (expected.command == "Backward" && currentZone == expected.zone)
            return Input.GetKeyDown(KeyCode.S);

        if (expected.command == "Stay" && currentZone == expected.zone)
        {
            return !Input.GetKey(KeyCode.W)
                && !Input.GetKey(KeyCode.S)
                && !Input.GetKey(KeyCode.A)
                && !Input.GetKey(KeyCode.D)
                && !Input.GetKey(KeyCode.Space)
                && !Input.anyKey;
        }

        return false;
    }

    AudioClip GetClipFromCommand(string command)
    {
        if (command == "Forward") return forwardClip;
        if (command == "Backward") return backwardClip;
        if (command == "Stay") return stayClip;
        return null;
    }

    void PuzzleFail()
    {
        Debug.Log("퍼즐 실패. 기억이 흩어졌습니다.");

        StartCoroutine(ShowFailFog());

        ResetAllDoors();
        lastSuccessZone = -1;

        currentIndex = 0;
        PuzzleStarted = false;

        if (playerStartPosition != null)
        {
            Debug.Log("[이동] 플레이어를 시작 위치로 복귀");
            player.position = playerStartPosition.position;
            player.rotation = playerStartPosition.rotation;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                player.position = playerStartPosition.position;
                cc.enabled = true;
            }
        }
        else
        {
            Debug.LogWarning("[경고] playerStartPosition 연결 안 됨!");
        }

        player.GetComponent<PlayerZoneState>().ExitPuzzleMode();
    }

    IEnumerator ShowFailFog()
    {
        Vector3 frontPosition = player.position + player.forward * 0.8f;
        frontPosition.y += 1.5f;
        failFog.transform.position = frontPosition;
        failFog.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        var vfx = failFog.GetComponent<UnityEngine.VFX.VisualEffect>();
        if (vfx != null) vfx.Play();

        failFog.SetActive(true);

        yield return new WaitForSeconds(2f);
        failFog.SetActive(false);
    }

    void ResetAllDoors()
    {
        foreach (var door in doors)
        {
            if (door != null)
                door.ResetDoor();
        }
        Debug.Log("[문 닫기] 모든 문 초기화 완료");
    }
}
