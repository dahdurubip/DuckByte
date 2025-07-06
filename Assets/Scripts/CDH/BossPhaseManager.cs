using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;

public class BossPhaseManager : MonoBehaviour
{
    [Header("기본 설정")]
    public float phaseInterval = 3f;
    public float shockwaveRadius = 5f;
    public int shockwaveMentalDamage = 20;
    public int currentPhase = 1;
    public float followDuration = 1f; // 경고 따라다니는 시간
    public float followSpeed = 5f;    // 따라다니는 부드러움 속도
    private bool isPaused = false;

    [Header("１ 페이즈")]
    public Transform player;          // 따라다닐 플레이어

    [Header("2 페이즈")]
    public GameObject burningGroundPrefab; // 불바닥 프리팹
    public float burnDuration = 3f;        // 불바닥 제거 시간
    public GameObject randomArea;

    [Header("3 페이즈")]
    public GameObject phase3Land;
    public GameObject phase2Land;
    public GameObject mentalImage;
    private Coroutine phase2Coroutine;

    [Header("스크립트")]
    [SerializeField] private DialogueManager dialogueManager; // 대사 출력 담당
    [SerializeField] private DialogueData dialogueData;       // 대사 데이터 보관소
    [SerializeField] private EggGroupManager eggGroupManager;   // 거미알 아이템 드롭 알 선택


    [Header("프리팹")]
    public GameObject rockPrefab;
    //public GameObject bossObject;
    public GameObject warningCirclePrefab;
    public GameObject shockwaveEffectPrefab;
    [SerializeField] AudioSource warnningAudio;
    [SerializeField] AudioSource bossHowlAudio;
    [SerializeField] GameObject bossTMCamera;
    [SerializeField] GameObject bossTM;
    [SerializeField] Collider FinalCollider;

    private int expectedEggCount = 5; // 알 개수 정확히 입력
    private int registeredEggCount = 0;


    //public Transform[] rockSpawnPoints;

    // 원하는 수치로 조정
    //public int spawnCount = 4; 
    //private float timer;

    //public Transform altar;              // 제단 위치
    //public float altarSafeRadius = 3f;   // 제단 보호 범위


    //// 보스 2페이즈
    //[SerializeField] GameObject burningGroundPrefab; // 불타는 바닥 프리팹
    //[SerializeField] float groundRadius = 2f;        // 데미지 범위 반지름
    //[SerializeField] float burnDuration = 3f;        // 몇 초 동안 유지할지


    //void Start()
    //{
    //    //Debug.Log(currentPhase);
    //    SetPhase(currentPhase);
    //}

    public void OnEggRegistered()
    {
        registeredEggCount++;
        //Debug.Log($"[Boss] 알 등록됨 ({registeredEggCount}/{expectedEggCount})");

        if (registeredEggCount == expectedEggCount)
        {
            //Debug.Log("[Boss] 모든 알 등록 완료 → 1페이즈 시작!");
            SetPhase(1);
        }
    }

    //void Update()
    //{
    //    timer += Time.deltaTime;
    //    //Debug.Log("Timer: " + timer);

    //    if (timer >= phaseInterval)
    //    {
    //        timer = 0;
    //        Debug.Log("코루틴 시작 조건 만족");

    //        switch (currentPhase)
    //        {
    //            case 1:
    //                StartCoroutine(Phase1Attack());
    //                break;
    //            case 2:
    //                StartCoroutine(Phase2Attack());
    //                break;
    //            case 3:
    //                StartCoroutine(Phase3Attack());
    //                break;
    //        }
    //    }
    //}

    //public void SetPhase(int phase)
    //{
    //    currentPhase = phase;
    //    timer = 0;
    //}



    public void SetPhase(int phase)
    {
        //if (phase <= currentPhase) return; // 이미 진행한 단계면 무시

        currentPhase = phase;
        //Debug.Log(currentPhase);
        //timer = 0;

        //if (eggGroupManager != null)
        //{
        //    if (currentPhase == 1)
        //        eggGroupManager.PickDropEgg(1); // 1페이즈용 알 중 랜덤 선택
        //    else if (currentPhase == 2)
        //        eggGroupManager.PickDropEgg(2); // 2페이즈용 알 중 랜덤 선택
        //    else if (currentPhase == 3)
        //        eggGroupManager.PickDropEgg(3); // 3페이즈용 알 중 랜덤 선택
        //}

        if (eggGroupManager != null)
        {
            if (currentPhase == 1)
            {
                eggGroupManager.PickDropEgg(1);   // 1페이즈용 알 중 랜덤 선택
                eggGroupManager.SetActiveEggs(1); // 1페이즈 알만 켜기
            }
            else if (currentPhase == 2)
            {
                eggGroupManager.PickDropEgg(2);   // 2페이즈용 알 중 랜덤 선택
                eggGroupManager.SetActiveEggs(2); // 2페이즈 알만 켜기
            }
            else if (currentPhase == 3)
            {
                eggGroupManager.PickDropEgg(3);   // 3페이즈용 알 중 랜덤 선택
                eggGroupManager.SetActiveEggs(3); // 3페이즈 알만 켜기
            }
        }

        switch (currentPhase)
        {
            case 1:
                StartCoroutine(Phase1Attack());
                break;
            case 2:
                StartCoroutine(Phase2Attack());
                break;
            case 3:
                StartCoroutine(Phase3Attack());
                break;
            case 4:
                bossClear();
                break;
        }
    }




    //IEnumerator Phase1Attack()
    //{
    //    Debug.Log("1페이즈 시작");
    //    Transform targetPoint = rockSpawnPoints[Random.Range(0, rockSpawnPoints.Length)];
    //    GameObject warning = Instantiate(warningCirclePrefab, targetPoint.position, Quaternion.identity);
    //    yield return new WaitForSeconds(1f);
    //    Destroy(warning);
    //    Instantiate(rockPrefab, targetPoint.position, Quaternion.identity);
    //}

    IEnumerator Phase1Attack()
    {
        //Debug.Log("Phase1Attack 시작");
        while (currentPhase == 1)
        {
            if (isPaused) { yield return null; continue; }

            //Debug.Log("Phase1Attack 반복 시작");
            // 1. 경고 프리팹 생성 (시작할 때 플레이어 위치)
            GameObject warning = Instantiate(warningCirclePrefab, player.position, Quaternion.Euler(90, 0, 0));
            warnningAudio.Play();

            float timer = 0f;

            while (timer < followDuration)
            {
                // 경고 프리팹이 플레이어 따라다니게
                warning.transform.position = player.position;

                timer += Time.deltaTime;
                yield return null;
            }

            // 2. 마지막 플레이어 위치 저장
            Vector3 dropPosition = player.position;

            // 3. 경고 제거
            Destroy(warning);

            //// 4. 돌을 마지막 위치 위로부터 생성
            //Instantiate(rockPrefab, dropPosition + Vector3.up * 5f, Quaternion.identity);
            //yield return new WaitForSeconds(0.5f);
            //Instantiate(rockPrefab, dropPosition + Vector3.up * 5f, Quaternion.identity);

            // 4. 돌을 마지막 위치 위로부터 생성
            GameObject rock1 = Instantiate(rockPrefab, dropPosition + Vector3.up * 5f, Quaternion.identity);
            Rigidbody rb1 = rock1.GetComponent<Rigidbody>();
            if (rb1 != null)
            {
                rb1.linearVelocity = Vector3.down * 10f; // 빠르게 낙하
            }

            yield return new WaitForSeconds(0.5f);

            GameObject rock2 = Instantiate(rockPrefab, dropPosition + Vector3.up * 5f, Quaternion.identity);
            Rigidbody rb2 = rock2.GetComponent<Rigidbody>();
            if (rb2 != null)
            {
                rb2.linearVelocity = Vector3.down * 20f;
            }


            //예측 드롭
            //Vector3 secondDrop = dropPosition + (player.forward * 2f);
            //Instantiate(rockPrefab, secondDrop + Vector3.up * 0.5f, Quaternion.identity);

            yield return new WaitForSeconds(1f); // 다음 행동까지 약간 기다림
        }

    }


    //IEnumerator Phase2Attack()
    //{
    //    while (currentPhase == 2)
    //    {
    //        if (isPaused) { yield return null; continue; }

    //        RockOff rockScript = rockPrefab.GetComponent<RockOff>();
    //        rockScript.bossPhaseManager = this; // 혹은 this.bossPhaseManager;

    //        List<GameObject> warnings = new List<GameObject>();

    //        //  랜덤으로 몇 개 생성할지 정하기
    //        //int spawnCount = 4; // 원하는 수치로 조정
    //        int spawnCount = Random.Range(5, 7); // 3~5개 사이 랜덤


    //        List<Transform> randomPoints = new List<Transform>(rockSpawnPoints);

    //        // 리스트 섞기 (Fisher-Yates 방식)
    //        for (int i = 0; i < randomPoints.Count; i++)
    //        {
    //            Transform temp = randomPoints[i];
    //            int randomIndex = Random.Range(i, randomPoints.Count);
    //            randomPoints[i] = randomPoints[randomIndex];
    //            randomPoints[randomIndex] = temp;
    //        }

    //        // 랜덤으로 선택된 지점에 워닝 생성
    //        for (int i = 0; i < spawnCount && i < randomPoints.Count; i++)
    //        {
    //            //warnings.Add(Instantiate(warningCirclePrefab, randomPoints[i].position, Quaternion.identity));

    //            //Vector3 warningPosition = randomPoints[i].position + Vector3.down * 4f;
    //            Vector3 warningPosition = randomPoints[i].position + Vector3.down * 4f;
    //            warnings.Add(Instantiate(warningCirclePrefab, warningPosition, Quaternion.Euler(90, 0, 0)));
    //        }

    //        yield return new WaitForSeconds(1f);

    //        foreach (var warning in warnings)
    //        {
    //            Destroy(warning);
    //        }

    //        // 같은 지점에 돌 생성
    //        for (int i = 0; i < spawnCount && i < randomPoints.Count; i++)
    //        {
    //            Instantiate(rockPrefab, randomPoints[i].position, Quaternion.identity);
    //        }
    //        //GameObject rock = Instantiate(rockPrefab, randomPoints[i].position, Quaternion.identity);
    //        yield return new WaitForSeconds(2f);
    //    }
    //}

    //IEnumerator Phase2Attack()
    //{
    //    while (currentPhase == 2)
    //    {
    //        if (isPaused) { yield return null; continue; }

    //        RockOff rockScript = rockPrefab.GetComponent<RockOff>();
    //        rockScript.bossPhaseManager = this;

    //        List<GameObject> warnings = new List<GameObject>();
    //        List<Vector3> rockPositions = new List<Vector3>();

    //        int spawnCount = Random.Range(5, 7);

    //        // BoxCollider로부터 랜덤 위치 추출
    //        BoxCollider area = randomArea.GetComponent<BoxCollider>();
    //        Vector3 center = area.center + area.transform.position;
    //        Vector3 size = area.size;

    //        for (int i = 0; i < spawnCount; i++)
    //        {
    //            float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
    //            float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
    //            Vector3 spawnPosition = new Vector3(randomX, area.transform.position.y, randomZ); // 필요 시 Y 보정

    //            Vector3 warningPosition = spawnPosition + Vector3.down * 4f;
    //            warnings.Add(Instantiate(warningCirclePrefab, warningPosition, Quaternion.Euler(90, 0, 0)));
    //            rockPositions.Add(spawnPosition);
    //        }

    //        yield return new WaitForSeconds(1f);

    //        foreach (var warning in warnings)
    //        {
    //            Destroy(warning);
    //        }

    //        foreach (var pos in rockPositions)
    //        {
    //            Instantiate(rockPrefab, pos, Quaternion.identity);
    //        }

    //        yield return new WaitForSeconds(2f);
    //    }
    //}

    IEnumerator Phase2Attack()
    {
        while (currentPhase == 2 || currentPhase == 3)
        {
            if (isPaused) { yield return null; continue; }

            RockOff rockScript = rockPrefab.GetComponent<RockOff>();
            rockScript.bossPhaseManager = this;

            List<GameObject> warnings = new List<GameObject>();
            List<Vector3> rockPositions = new List<Vector3>();

            //int spawnCount = Random.Range(5,7);
            int spawnCount = 0;
            float minDistance = 4.0f;         // 돌 간 최소 거리

            if (currentPhase == 2)
            {
                spawnCount = Random.Range(7, 10);
                minDistance = 4.0f;
            }
            else if (currentPhase == 3)
            {
                spawnCount = Random.Range(5, 7);
                minDistance = 3.5f; // 3페이즈에서 좀 더 좁게 떨어지도록 설정
            }

            int maxAttempts = 50;             // 무한루프 방지 시도 제한

            // BoxCollider로부터 범위 계산
            BoxCollider area = randomArea.GetComponent<BoxCollider>();
            Vector3 center = area.center + area.transform.position;
            Vector3 size = area.size;

            int i = 0;
            int attempts = 0;

            warnningAudio.Play();
            while (i < spawnCount && attempts < maxAttempts)
            {
                attempts++;

                float randomX = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
                float randomZ = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
                Vector3 spawnPos = new Vector3(randomX, area.transform.position.y, randomZ);

                bool tooClose = false;
                foreach (var pos in rockPositions)
                {
                    if (Vector3.Distance(pos, spawnPos) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                Vector3 warningPos = spawnPos + Vector3.down * 4f;
                warnings.Add(Instantiate(warningCirclePrefab, warningPos, Quaternion.Euler(90, 0, 0)));
                rockPositions.Add(spawnPos);
                i++;
            }

            // 로그 추가 (디버깅용)
            //if (i < spawnCount)
            //{
            //    Debug.LogWarning($"스폰 제한에 도달하여 {i}/{spawnCount}개만 생성됨");
            //}

            yield return new WaitForSeconds(1f);

            foreach (var warning in warnings)
            {
                Destroy(warning);
            }

            foreach (var pos in rockPositions)
            {
                Instantiate(rockPrefab, pos, Quaternion.identity);
            }

            yield return new WaitForSeconds(2f);
        }
    }


    IEnumerator Phase3Attack()
    {

        phase3Land.SetActive(true);
        phase2Land.SetActive(false);
        mentalImage.SetActive(true);

        // Phase2 코루틴 중지
        //if (phase2Coroutine != null)
        //{
        //    StopCoroutine(phase2Coroutine);
        //    phase2Coroutine = null;
        //}
        //if (phase2Coroutine == null)
        //{
        //    phase2Coroutine = StartCoroutine(Phase2Attack());
        //}

        while (currentPhase == 3)
        {
            if (isPaused) { yield return null; continue; }



            //1.경고음 재생
            //Debug.Log("충격파 등장 위이잉");
            //2.카메라 흔들림
            //Debug.Log("카메라 흔들흔들");

            yield return new WaitForSeconds(1f);


            //3.충격파 이펙트 발생
            Instantiate(shockwaveEffectPrefab, player.transform.position, Quaternion.identity);

            // 플레이어가 제단 반경 안에 있는지 확인
            //float distanceToAltar = Vector3.Distance(player.position, altar.position);

            // 안전 반경 밖이면 정신력 데미지
            //if (distanceToAltar > altarSafeRadius)
            //{
                PlayerMental mental = player.GetComponent<PlayerMental>();
                if (mental != null && !mental.isHealing)
                {
                    //Debug.Log("안전 반경 밖임");
                    //4.정신력 데미지
                    mental.TakeMentalDamage(shockwaveMentalDamage);
                    //5.카메라 색상 왜곡 효과(포스트 프로세싱)
                }
           // }
            yield return new WaitForSeconds(2f);
        }
    }

    public void triggerPhaseTransition(int nextPhase)
    {
        StartCoroutine(handlePhaseTransition(nextPhase));
    }

    IEnumerator handlePhaseTransition(int nextPhase)
    {
        isPaused = true;

        // 연출 전 준비 시간
        yield return new WaitForSeconds(0.5f);

        // 페이즈 넘어가는 연출 시작
        //Debug.Log("보스 : 으윽");
        // 보스 그르릉 소리 추가
        bossHowlAudio.Play();

        if (dialogueData.phaseDialogues.ContainsKey(nextPhase))
        {
            foreach (string line in dialogueData.phaseDialogues[nextPhase])
            {
                yield return StartCoroutine(dialogueManager.ShowDialogue("boss", line));
            }
        }

        //string[] lines = new string[]
        //{
        //"oh",
        //"no",
        //"fuck"
        //};

        //foreach (string line in lines)
        //{
        //    yield return StartCoroutine(dialogueManager.ShowDialogue("보스", line));
        //}

        // 연출 시간
        //yield return new WaitForSeconds(2f);

        isPaused = false;
        SetPhase(nextPhase);
    }
    
    void bossClear()
    {
        //Debug.Log("보스전 클리어");
        //SceneManager.LoadScene("button");
        //SceneLoad.LoadSceneWithLoading("button");
        mentalImage.SetActive(false);
        bossTMCamera.SetActive(true);
        bossTM.SetActive(true);
        FinalCollider.enabled = false;
    }

}
