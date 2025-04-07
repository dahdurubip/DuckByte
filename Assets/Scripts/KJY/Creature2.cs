using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Creature2 : MonoBehaviour
{

    //[SerializeField] private float growScaleAmount = 1.2f; // 성장 시 크기 비율
    [SerializeField] private Creature2Manager creature2Manager;
    [SerializeField] private Player player;

    //private float moveSpeed = 2f;
    //private float teleportDistance = 5f;

    //AI 이동경로
    [SerializeField] private Transform[] wayPoint;
    [SerializeField] private NavMeshAgent navMeshAgent;

    //크리처 상태
    private enum creatureState {Patrol, Pursuit, Attack, Idle};
    private creatureState currentState;
    private bool isAttacking;
    private int currentPatrolIndex = 0;
    private bool patrolling = true;
    private float idleTime = 1f;
    private float Timer = 0f;
    private float attackRange = 2f;
    private float detectionRange = 10f;
    private float damage = 10f;


    //크리처 액션
    private Animator animator;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        isAttacking = false;
        currentState = creatureState.Patrol;
        Creature2Patrol();
    }

    private void Update()
    {


        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= detectionRange && !isAttacking)
        {
            currentState = creatureState.Pursuit;
        }

        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            if (!isAttacking)
            {
                currentState = creatureState.Attack;
                Creature2Attack();
            }
        }

        if (creature2Manager.CanTeleportation)
        {
            Creature2Teleportation();
            creature2Manager.CanTeleportation = false;
        }

        if (creature2Manager.CanGrow)
        {
            GrowCreature(creature2Manager.detectionCount);
            creature2Manager.CanGrow = false;
        }

        switch (currentState)
        {
            case creatureState.Patrol:
                Creature2Patrol();
                break;
            case creatureState.Attack:
                Creature2Attack();
                break;
            case creatureState.Idle:
                Creature2Idle();
                break;
            case creatureState.Pursuit:
                Creature2Pursuit();
                break;
        }



    }

    //public void UpdateStats(float speed, float teleportDist)
    //{
    //    moveSpeed = speed;
    //    teleportDistance = teleportDist;
    //}

    // 예: 플레이어 추적, 순간이동 등에서 moveSpeed, teleportDistance 활용

    //메니저가 알려주면 성장하는 함수를 호출 // 순간이동하라는 지시를 내리면 순간이동한다.

    private void Creature2Teleportation()
    {
        navMeshAgent.isStopped = true;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);

        Vector3 teleportOffset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        transform.position = player.transform.position + teleportOffset;

        // 즉시 플레이어 방향으로 회전
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));


        currentState = creatureState.Attack;
        Debug.Log("순간이동함");
    }

    private void GrowCreature(int _Cnt)
    {
        //transform.localScale *= growScaleAmount;
        Debug.Log("크리처가 성장했습니다!");

        if (_Cnt == 3)
        {
            //attack함수에 있는 데미지의 수치 바꾼다.
            damage = 30f;
            //길에 있는 벽이 3개 완전히 올라간다.
            creature2Manager.wallmove = true;
        }
        else if (_Cnt == 6)
        {
            //attack함수에 있는 데미지의 수치 바꾼다.
            damage = 60f;
            //몸에 빨간 빛이 생긴다.
            //이동가능한 벽들이 완전히 올라간다.   벽의 위치를 13으로 고정
            creature2Manager.wallmove = true;

            //플레이어의 위치로 이동한다.
            currentState = creatureState.Pursuit;
        }
        else if (_Cnt >= 9)
        {
            //attack함수에 있는 데미지의 수치 바꾼다.
            damage = 90f;
            //몸에 보라 빛이 생긴다.
            //바로 순간이동 한다.
            Creature2Teleportation();
            creature2Manager.CanTeleportation = false;
            //이동 불가능한 벽 포함 완전히 올라간다.
            creature2Manager.wallmove = true;
        }
        
    }

    private void Creature2Patrol()
    {
        //루틴대로 걸어가지만 만약에 앞에 막혀있으면 반대로 첫번째 지점으로 돌아가기

        if (wayPoint.Length == 0) return;

        navMeshAgent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);

        navMeshAgent.destination = wayPoint[currentPatrolIndex].position;

        //순찰 지점에 도착했을 때 Idle상태로 전한
        if(navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending)
        {
            currentState = creatureState.Idle;
        }
    }

    private void TheNextWayPoint()
    {
        if(patrolling)
        {
            ++currentPatrolIndex;
            if(currentPatrolIndex >= wayPoint.Length)
            {
                currentPatrolIndex = wayPoint.Length - 2;
                patrolling = false;
            }
        }
        else
        {
            --currentPatrolIndex;
            if(currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1;
                patrolling = true;
            }
        }

        currentState = creatureState.Patrol;
    }

    private void Creature2Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        navMeshAgent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isRun", false);
        animator.SetTrigger("Attack");

        Vector3 dir = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        //데미지크드 추가해야 함

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

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if(distanceToPlayer <= attackRange)
        {
            player.TakeDamage(damage);
            Creature2Attack();
        }
        else
        {
            if(distanceToPlayer <= detectionRange)
            {
                currentState = creatureState.Pursuit;
            }
            else
            {
                currentState = creatureState.Patrol;
            }
            animator.SetBool("isWalking", true);
        }
    }

    private void Creature2Pursuit()
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.destination = player.transform.position;
        navMeshAgent.speed = 5.0f;
        animator.SetBool("isRun", true);
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", false);
    }

    private void Creature2Idle()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isIdle", true);

        Timer += Time.deltaTime;
        if(Timer >= idleTime)
        {
            Timer = 0f;
            TheNextWayPoint();
        }

    }
}
