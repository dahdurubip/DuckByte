using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    public GameManager GM;
    public NavMeshAgent NMA;
    public Transform player;
    public PlayerMovement pM;
    public Player playerScripts;

    public float chaseSpeed = 3.0f;                 // 추적 속도
    public float patrolSpeed = 2.0f;                // 배회 속도
    public float suspicionRange = 5f;               // 의심 인식 범위
    public float panicRange = 2f;                   // 패닉 가능성 범위
    public float panicTime = 4f;                    // 패닉까지 걸리는 시간

    private Vector3 targetPosition;                 // 아이템 상호작용 같이 소리난 곳을 받아올 위치 값
    private bool isChasing = false;                 // 몬스터의 추적 상태
    private bool isPatrolling = false;              // 몬스터의 배회 상태
    private bool isSuspicious = false;              // 몬스터의 의심 상태
    private Coroutine suspicionCoroutine;           // 의심 코루틴
    private Coroutine panicCoroutine;               // 배회 코루틴

    public Animator anim;
    [SerializeField] AudioSource idleSound;



    private void Update()
    {
        if (pM.run && !playerScripts.isInside)
        {
            StartChasingPlayer();
        }
    }

    // 몬스터의 추적 모드
    private enum ChaseMode
    {
        StaticTarget, // 고정된 위치 추적 ex) 소리 발생 위치
        FollowPlayer  // 플레이어 실시간 추적
    }

    private ChaseMode currentChaseMode;

    void Start()
    {
        GM.OnSoundEmitted += RespondToSound;
        Patrol();
    }

    void OnDestroy()
    {
        GM.OnSoundEmitted -= RespondToSound;
    }


    // 고정된 소리 발생 위치를 추적할때 쓸 함수 // 이벤트 함수
    void RespondToSound(Vector3 soundPosition)
    {
        targetPosition = soundPosition;
        currentChaseMode = ChaseMode.StaticTarget;
        isChasing = true;
        NMA.isStopped = false;

        setRun();

        StopAllCoroutines();
        StartCoroutine(ChaseSound());
    }

    // 실시간으로 플레이어를 추적할때 쓸 함수
    void StartChasingPlayer()
    {
        if (isChasing) return; // 이미 추적중이면 리턴한다.

        currentChaseMode = ChaseMode.FollowPlayer;
        isChasing = true;
        NMA.isStopped = false;

        // 추적하는 애니메이션 넣기 - Run 애니메이션
        setRun();

        StopAllCoroutines();
        StartCoroutine(ChaseSound());
    }

    private IEnumerator ChaseSound()
    {
        //Debug.Log("추적 시작");
        isPatrolling = false;
        NMA.speed = chaseSpeed;
        //NMA.SetDestination(targetPosition);

        while (isChasing)
        {
            //if (NMA.pathPending)
            //{
            //    yield return null;
            //    continue;
            //}

            //if (NMA.remainingDistance <= 0.5f)
            //{
            //    Debug.Log("추적 종료! 배회 시작");
            //    isChasing = false;
            //    Patrol();
            //    yield break;
            //}

            //float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            //// 패닉 조건 체크
            //if (distanceToPlayer <= panicRange && !playerMovement.IsMoving && panicCoroutine == null)
            //{
            //    panicCoroutine = StartCoroutine(TriggerPanic());
            //}

            //yield return null;


            ///////////////////////////////////////////////////////////////////////

            //if (!NMA.pathPending) // 경로계산 중이 아닐 시 다음을 실행
            //{
            //    // 현재 모드에 따라 다르게 설정 followplayer 모드면 playerposition 아닐 시는 targetposition
            //    Vector3 destination = currentChaseMode == ChaseMode.FollowPlayer ? player.position : targetPosition;
            //    NMA.SetDestination(destination);


            //    // 현재 몬스터와 플레이어 사이의 거리를 측정
            //    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            //    // 패닉 조건 위의 거리가 패닉이 오늘 거리안이고 플레이어가 움직이지않고 패닉 코루틴이 널일시 다음 실행
            //    if (distanceToPlayer <= panicRange && !playerMovement.IsMoving && panicCoroutine == null)
            //    {
            //        panicCoroutine = StartCoroutine(TriggerPanic());
            //    }

            //    // 추적모드가 플레이어 모드이고 위의 거리가 인식범위보다 멀어지면 추적을 포기하고 배회 시작
            //    if (currentChaseMode == ChaseMode.FollowPlayer && distanceToPlayer > suspicionRange * 3f)
            //    {
            //        Debug.Log("플레이어가 멀어짐, 추적 종료");
            //        isChasing = false;
            //        Patrol();
            //        yield break;
            //    }

            //    // 추적모드가 소리모드이고 목적지에 도착하면 배회 시작
            //    if (currentChaseMode == ChaseMode.StaticTarget && NMA.remainingDistance <= 0.5f)
            //    {
            //        Debug.Log("도착 완료, 배회 재시작");
            //        isChasing = false;
            //        Patrol();
            //        yield break;
            //    }
            //}
            //yield return null;

            //////////////////////////////////////////////////////////////////////////////////////

            // 현재 모드에 따라 목적지 설정
            if (currentChaseMode == ChaseMode.FollowPlayer)
            {
                NMA.SetDestination(player.position); // 항상 최신 위치로 갱신
            }
            else if (!NMA.pathPending) // 정적 추적일 때만 pathPending 고려
            {
                NMA.SetDestination(targetPosition);
            }

            // 거리 계산
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // 패닉 조건 체크
            if (distanceToPlayer <= panicRange && !pM.IsMoving && panicCoroutine == null)
            {
                panicCoroutine = StartCoroutine(TriggerPanic());
            }

            // 추적 종료 조건
            if (currentChaseMode == ChaseMode.FollowPlayer && distanceToPlayer > suspicionRange * 2f)
            {
                //Debug.Log("플레이어가 멀어짐, 추적 종료");
                isChasing = false;
                StartCoroutine(LookAroundCoroutine());
                //Patrol();
                yield break;
            }

            if (currentChaseMode == ChaseMode.StaticTarget && !NMA.pathPending && NMA.remainingDistance <= 0.5f)
            {
               // Debug.Log("도착 완료, 배회 재시작");
                isChasing = false;
                StartCoroutine(LookAroundCoroutine());
                //Patrol();
                yield break;
            }

            yield return null;

        }
    }

    private IEnumerator LookAroundCoroutine()
    {
        NMA.isStopped = true;
        idleSound.Play();
        //float timer = 0f;
        //float duration = 2f;

        setLook();
        //while (timer < duration)
        //{
        //    //// 랜덤하게 고개를 좌우로 돌리는 연출
        //    //float angle = Mathf.Sin(Time.time * 2f) * 30f;
        //    //transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + angle * Time.deltaTime, 0f);


        //    timer += Time.deltaTime;
        //    yield return null;
        //}
        yield return new WaitForSeconds(2f);
        NMA.isStopped = false;
        Patrol(); // 또는 StartCoroutine(PatrolCoroutine());
    }

    private IEnumerator TriggerPanic()
    {
        float elapsed = 0f;
        //Debug.Log("패닉 카운트다운 시작");
        while (elapsed < panicTime)
        {
            if (pM.IsMoving || Vector3.Distance(transform.position, player.position) > panicRange)
            {
                //Debug.Log("패닉 조건 해제");
                panicCoroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        //Debug.Log("패닉 발동! 추적 시작");
        GM.TriggerPanicEffect();
        isChasing = true;
        //StopAllCoroutines();
        //StartCoroutine(ChaseSound());
        //RespondToSound(player.position);
        StartChasingPlayer();
        panicCoroutine = null;
    }

    void Patrol()
    {
        if (!isPatrolling)
        {
            //Debug.Log("배회 시작!");
            isPatrolling = true;
            NMA.speed = patrolSpeed;

            // 배회 애니메이션 넣기 - walk 모션
            //setWalk();

            StartCoroutine(PatrolCoroutine());
        }
    }

    private IEnumerator PatrolCoroutine()
    {
        while (isPatrolling)
        {
            //Vector3 patrolPosition = new Vector3(
            //    Random.Range(-10f, 10f),
            //    transform.position.y,
            //    Random.Range(-10f, 10f)
            //);

            Vector3 patrolPosition = GetRandomNavMeshPosition(transform.position, 50f);

            NMA.SetDestination(patrolPosition);
            setWalk();
            //Debug.Log($"새로운 배회 목적지 설정: {patrolPosition}");

            while (true)
            {
                if (NMA.pathPending)
                {
                    yield return null;
                    continue;
                }

                float distanceToPlayer = Vector3.Distance(transform.position, player.position);


                // 패닉
                if (distanceToPlayer <= panicRange && !pM.IsMoving && panicCoroutine == null && !playerScripts.isInside)
                {
                    panicCoroutine = StartCoroutine(TriggerPanic());
                }


                // 의심
                if (distanceToPlayer <= suspicionRange && pM.IsMoving && !playerScripts.isInside && !pM.isCrouching)
                {
                   // Debug.Log("의심 거리 들어왔음");
                    if (!isSuspicious) // (!isSuspicious && suspicionCoroutine == null) 수정할지 고민
                    {
                        NMA.isStopped = true;
                        idleSound.Play();
                        isSuspicious = true;
                        suspicionCoroutine = StartCoroutine(HandleSuspicion());
                    }
                }
                //else
                //{
                //    if (isSuspicious)
                //    {
                //        isSuspicious = false;
                //        NMA.isStopped = false;
                //        if (suspicionCoroutine != null)
                //        {
                //            StopCoroutine(suspicionCoroutine);
                //            suspicionCoroutine = null;
                //        }
                //    }
                //}

                if (NMA.remainingDistance < 0.5f && NMA.velocity.magnitude < 0.1f)
                {
                    //Debug.Log("배회 목적지 도착!");
                    setLook();
                    break;
                }

                yield return null;
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator HandleSuspicion()
    {
        //Debug.Log("플레이어 의심 중...");
        //transform.LookAt(player);
        //yield return new WaitForSeconds(2f);

        //if (Vector3.Distance(transform.position, player.position) <= suspicionRange && playerMovement.IsMoving)
        //{
        //    Debug.Log("플레이어 추적 시작!");
        //    //isChasing = true;
        //    //isPatrolling = false;
        //    //StopAllCoroutines();
        //    //StartCoroutine(ChaseSound());
        //    RespondToSound(player.position);
        //}
        //else
        //{
        //    Debug.Log("의심해제 다시 배회시작");
        //    NMA.isStopped = false;
        //}

        //    isSuspicious = false;

        //Debug.Log("플레이어 의심 중...");

        setLook();

        // 플레이어가 움직이는 시간을 계산 할 변수
        float timer = 0f;
        // 플레이어가 멈춰도 유예시간을 줄 변수
        float lookIngTimer = 0f;
        float lookIngDuration = 1f;
        // 플레이어가 멀어지면 의심 해제 될 거리
        float maxDistance = suspicionRange * 2f;

        while (timer < 1f)
        {
            // 계속 바라보게 만듦
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            // 조건 유지 중이면 타이머 증가
            if (Vector3.Distance(transform.position, player.position) <= suspicionRange && pM.IsMoving)
            {
                timer += Time.deltaTime;
                lookIngTimer = 0f;
            }
            else if (Vector3.Distance(transform.position, player.position) > maxDistance)
            {
               // Debug.Log("플레이어가 너무 멀어져서 의심 해제");
                isSuspicious = false;
                NMA.isStopped = false;
                setWalk();
                yield break;
            }
            else
            {
                lookIngTimer += Time.deltaTime;
                if (lookIngTimer >= lookIngDuration)
                {
                   // Debug.Log("의심 조건 해제됨");
                    isSuspicious = false;
                    NMA.isStopped = false;
                    setWalk();
                    yield break;
                }
            }

            yield return null;
        }

        //Debug.Log("플레이어 추적 시작!");
        //RespondToSound(player.position);
        StartChasingPlayer();
        isSuspicious = false;
    }

    private Vector3 GetRandomNavMeshPosition(Vector3 center, float range)
    {
        //for (int i = 0; i < 10; i++) // 최대 10번 시도 // 해당 부분은 장애물이 많은 곳에서 적절한 위치를 못 뽐을 시를 방지하기 위해 사용하는 것이 적절 하지만 속도가 조금 느림
        //{
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += center;


        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
        //}

        // 실패 시 현재 위치 반환
        return center;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("몬스터와 충돌! 플레이어 즉사");

            // 공격 애니메이션 넣기 - attack 트리거
            anim.SetTrigger("attackTrigger");
            // 공격 소리
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.m1Attack);


            //GM.KillPlayer();
            playerScripts.TakeDamage(100);
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        //Debug.Log("몬스터와 충돌! 플레이어 즉사");

    //        // 공격 애니메이션 넣기 - attack 트리거
    //        anim.SetTrigger("attackTrigger");
    //        // 공격 소리
    //        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.m1Attack);


    //        //GM.KillPlayer();
    //        playerScripts.TakeDamage(100);
    //    }
    //}

    void setBool()
    {
        anim.SetBool("isWalk", false);
        anim.SetBool("isRun", false);
        anim.SetBool("isLook", false);
    }

    void setRun()
    {
        setBool();
        anim.SetBool("isRun", true);
        //Debug.Log("달리기시작했음");

    }

    void setWalk()
    {
        setBool();
        anim.SetBool("isWalk", true);
       // Debug.Log("걷기시작했닭");
    }

    void setLook()
    {
        setBool();
        anim.SetBool("isLook", true);
    }
}
