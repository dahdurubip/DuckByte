using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossPhaseManager : MonoBehaviour
{
    public GameObject rockPrefab;
    public GameObject warningCirclePrefab;
    public GameObject shockwaveEffectPrefab;

    public Transform[] rockSpawnPoints;

    public float phaseInterval = 3f;
    public float shockwaveRadius = 5f;
    public int shockwaveMentalDamage = 20;

    public int currentPhase = 1;
    // 원하는 수치로 조정
    //public int spawnCount = 4; 
    private float timer;



    public Transform player;          // 따라다닐 플레이어

    public float followDuration = 1f; // 경고 따라다니는 시간
    public float followSpeed = 5f;    // 따라다니는 부드러움 속도


    void Start()
    {
        Debug.Log(currentPhase);
        SetPhase(currentPhase);
    }

    void Update()
    {
        timer += Time.deltaTime;
        Debug.Log("Timer: " + timer);

        if (timer >= phaseInterval)
        {
            timer = 0;
            Debug.Log("코루틴 시작 조건 만족");

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
            }
        }
    }

    public void SetPhase(int phase)
    {
        currentPhase = phase;
        timer = 0;
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
        // 1. 경고 프리팹 생성 (시작할 때 플레이어 위치)
        GameObject warning = Instantiate(warningCirclePrefab, player.position, Quaternion.identity);

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

        // 4. 돌을 마지막 위치 위로부터 생성
        Instantiate(rockPrefab, dropPosition + Vector3.up * 0.5f, Quaternion.identity);

        yield return new WaitForSeconds(1.5f); // 다음 행동까지 약간 기다림
    }


    IEnumerator Phase2Attack()
    {
        List<GameObject> warnings = new List<GameObject>();

        //  랜덤으로 몇 개 생성할지 정하기
        //int spawnCount = 4; // 원하는 수치로 조정
        int spawnCount = Random.Range(2, 4); // 3~5개 사이 랜덤


        List<Transform> randomPoints = new List<Transform>(rockSpawnPoints);

        // 리스트 섞기 (Fisher-Yates 방식)
        for (int i = 0; i < randomPoints.Count; i++)
        {
            Transform temp = randomPoints[i];
            int randomIndex = Random.Range(i, randomPoints.Count);
            randomPoints[i] = randomPoints[randomIndex];
            randomPoints[randomIndex] = temp;
        }

        // 랜덤으로 선택된 지점에 워닝 생성
        for (int i = 0; i < spawnCount && i < randomPoints.Count; i++)
        {
            warnings.Add(Instantiate(warningCirclePrefab, randomPoints[i].position, Quaternion.identity));
        }

        yield return new WaitForSeconds(1f);

        foreach (var warning in warnings)
        {
            Destroy(warning);
        }

        // 같은 지점에 돌 생성
        for (int i = 0; i < spawnCount && i < randomPoints.Count; i++)
        {
            Instantiate(rockPrefab, randomPoints[i].position, Quaternion.identity);
        }
    }

    IEnumerator Phase3Attack()
    {
        // 보스(해당 스크립트가 붙은 오브젝트)의 현재 위치를 저장
        Vector3 center = transform.position;
        // 저장한 곳에 경고 프리팹을 생성
        GameObject warning = Instantiate(warningCirclePrefab, center, Quaternion.identity);
        // 경고 프리팹의 크기를 충격파 범위에 맞게 키움
        // shockwaveRadius * 2 : 충격파의 지 름에 해당하는 크기로 설정
        warning.transform.localScale = Vector3.one * shockwaveRadius * 2f;
        // 1초간 대기, 경고 시간이자 플레이어의 회피 유도 타이밍
        yield return new WaitForSeconds(1f);
        // 경고 프리팹 제거
        Destroy(warning);

        // 충격파 시각 이펙트(프리팹) 실행
        Instantiate(shockwaveEffectPrefab, center, Quaternion.identity);

        // 반지름(shockwaveRadius)만큼의 구를 만들어 범위 안의 오브젝트들 감지
        // 보스를 중심으로 피격 판정 범위 설정
        Collider[] hits = Physics.OverlapSphere(center, shockwaveRadius);

        // 감지된 오브젝트들 각각에 대해 반복검사
        foreach (var hit in hits)
        {
            // 감지된 오브젝트의 Tag가 플레이어인지 확인
            if (hit.CompareTag("Player"))
            {
                //// 플레이어가 PlayerMental스크립트를 가지고 있다면 저장
                //var mental = hit.GetComponent<PlayerMental>();
                //// 스크립트를 가지고 있는 경우 TakeMentalDamage함수를 통해 정신력 데미지를 부여
                //if (mental != null)
                //{
                //    mental.TakeMentalDamage(shockwaveMentalDamage);
                //}
            }
        }
    }
}
