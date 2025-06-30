using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.Audio;

public class Creature2 : MonoBehaviour
{
    [Header("Defalut Setting")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;
    [SerializeField] private AudioClip shutClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip walkClip;

    [Header("AI Settings")]
    [SerializeField] private Transform[] wayPoint;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float pursuitSpeed = 10.0f;

    //크리처 상태
    [Header("Combat Stats")]
    [SerializeField] private float idleTime = 1f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    public float damage = 15f;

    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 2.0f; // 레이캐스트 감지 거리

    private enum CreatureState { Patrol, Pursuit, Attack, Idle };
    private CreatureState currentState;

    private bool isAttacking;
    private int currentPatrolIndex = 0;
    private bool patrollingForward = true;
    private float idleTimer = 0f;
    private bool isReversingCooldown = false;

    //크리처 애니메이션
    private Animator animator;
    private AudioSource audioSource;

    //flashTimer 활성화 여부
    public bool flashOn;


    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        audioSource.Stop();
        isAttacking = false;
        currentState = CreatureState.Patrol;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (isAttacking)
        {
            //만약에 공격중이면 건너뛰기
        }
        else if (flashOn)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = CreatureState.Attack;
            }
            else
            {
                currentState = CreatureState.Pursuit;
            }
        }
        else
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = CreatureState.Attack;
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = CreatureState.Pursuit;
            }
            else
            {
                if (currentState == CreatureState.Pursuit || (currentState != CreatureState.Idle && currentState != CreatureState.Patrol))
                {
                    currentState = CreatureState.Patrol;
                }
            }
        }

        switch (currentState)
        {
            case CreatureState.Patrol:
                Creature2Patrol();
                break;
            case CreatureState.Attack:
                Creature2Attack();
                break;
            case CreatureState.Idle:
                Creature2Idle();
                break;
            case CreatureState.Pursuit:
                Creature2Pursuit();
                break;
        }



    }

    private void Creature2Patrol()
    {
        //루틴대로 걸어가지만 만약에 앞에 막혀있으면 반대로 첫번째 지점으로 돌아가기

        navMeshAgent.speed = patrolSpeed;

        if (wayPoint.Length == 0)
        {
            if (currentState != CreatureState.Idle) currentState = CreatureState.Idle;
            return;
        }

        // [핵심 로직 추가] 전방에 'Wall' 태그를 가진 장애물이 있는지 확인
        RaycastHit hit;
        Vector3 raycastOrigin = transform.position + Vector3.up * 0.5f; // 레이 시작 위치를 살짝 위로 조정
        Vector3 direction = transform.forward; // AI가 바라보는 정면 방향

        // isReversingCooldown은 이전에 추가했던 '무한 반복 방지' 플래그입니다. 그대로 사용합니다.
        if (!isReversingCooldown && Physics.Raycast(raycastOrigin, direction, out hit, raycastDistance))
        {
            // 레이캐스트에 부딪힌 물체의 태그가 'Wall'이라면
            if (hit.collider.CompareTag("Wall"))
            {
                //Debug.Log("전방에 'Wall' 장애물 감지! 순찰 방향을 반대로 전환합니다.");
                ReversePatrolDirection(); // 방향 전환 함수 호출
                return; //이번 프레임의 나머지 순찰 로직은 건너뜁니다.
            }
        }

        //디버깅용: 에디터에서 레이캐스트를 시각적으로 확인
        Debug.DrawRay(raycastOrigin, direction * raycastDistance, Color.red);



        navMeshAgent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);
        animator.SetBool("isRun", false);

        if (audioSource.clip != walkClip || !audioSource.isPlaying)
        {
            audioSource.clip = walkClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        //waypoint 없을 경우
        if (currentPatrolIndex < 0 || currentPatrolIndex >= wayPoint.Length || wayPoint[currentPatrolIndex] == null)
        {
            Debug.LogWarning("Creature2: Invalid waypoint or index. Resetting patrol index.");
            currentPatrolIndex = 0;
            if (wayPoint.Length == 0 || wayPoint[currentPatrolIndex] == null)
            {
                if (currentState != CreatureState.Idle) currentState = CreatureState.Idle;
                return;
            }
        }


        navMeshAgent.destination = wayPoint[currentPatrolIndex].position;

        //순찰 지점에 도착했을 때 Idle상태로 전한
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.5f)
        {
            currentState = CreatureState.Idle;
            idleTimer = 0f;
        }
    }

    private void ReversePatrolDirection()
    {
        //방향을 뒤집습니다.
        patrollingForward = !patrollingForward;
        //다음 웨이포인트를 계산합니다.
        TheNextWayPoint();
        //짧은 쿨타임을 시작하여 무한정 방향을 바꾸는 것을 방지합니다.
        StartCoroutine(ReverseCooldown());
    }

    private IEnumerator ReverseCooldown()
    {
        isReversingCooldown = true;
        yield return new WaitForSeconds(1.0f); //1초 동안은 다시 방향을 바꾸지 않습니다.
        isReversingCooldown = false;
    }


    private void TheNextWayPoint()
    {
        if (wayPoint.Length == 0) return;

        if (patrollingForward)
        {
            ++currentPatrolIndex;
            if (currentPatrolIndex >= wayPoint.Length)
            {
                currentPatrolIndex = wayPoint.Length > 1 ? wayPoint.Length - 2 : 0;
                patrollingForward = false;
            }
        }
        else
        {
            --currentPatrolIndex;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = wayPoint.Length > 1 ? 1 : 0;
                patrollingForward = true;
            }
        }

        currentState = CreatureState.Patrol;
    }

    private void Creature2Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        navMeshAgent.isStopped = true;

        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);
        animator.SetTrigger("Attack");

        audioSource.Stop();
        audioSource.PlayOneShot(shutClip);

        if (player != null) player.StartCoroutine(player.PlayerHitEffect());
        if (itemmanager != null && itemmanager.currentItem != null && !itemmanager.currentItem.CompareTag("Flashlight"))
        {
            itemmanager.DropCurrentItem();
        }
        if (camShake != null) camShake.StartCoroutine(camShake.Shake(0.2f, 0.3f));

        if (player != null)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * navMeshAgent.angularSpeed);
        }
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(1f);
        isAttacking = false;
    }

    public void EndAttack()
    {
        isAttacking = false;
        navMeshAgent.isStopped = false;

        if (player == null)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= attackRange)
        {
            if (player != null) player.TakeDamage(damage);
            //Debug.Log("Damage");
            //Creature2Attack();
        }

        if (flashOn)
        {
            if (distanceToPlayer <= attackRange)
            {
                Creature2Attack();
            }
            else
            {
                currentState = CreatureState.Pursuit;
            }
        }
        else
        {
            if (distanceToPlayer <= attackRange)
            {
                Creature2Attack();
            }
            else if (distanceToPlayer <= detectionRange)
            {
                currentState = CreatureState.Pursuit;
            }
            else
            {
                currentState = CreatureState.Patrol;
            }
        }

        UpdateAnimatorForState(currentState);
    }

    private void UpdateAnimatorForState(CreatureState targetState)
    {
        animator.SetBool("isIdle", targetState == CreatureState.Idle);
        animator.SetBool("isWalking", targetState == CreatureState.Patrol);
        animator.SetBool("isRun", targetState == CreatureState.Pursuit);
    }


    private void Creature2Pursuit()
    {
        navMeshAgent.speed = pursuitSpeed;

        if (player == null)
        {
            currentState = CreatureState.Patrol;
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.destination = player.transform.position;

        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", true);

        if (audioSource.clip != runClip || !audioSource.isPlaying)
        {
            audioSource.clip = runClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Creature2Idle()
    {
        navMeshAgent.isStopped = true;

        animator.SetBool("isIdle", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);

        if (audioSource.isPlaying && (audioSource.clip == walkClip || audioSource.clip == runClip))
        {
            audioSource.Stop();
        }

        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            TheNextWayPoint();
        }

    }

}


