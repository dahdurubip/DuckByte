using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class Creature2 : MonoBehaviour
{
    [Header("Defalut Setting")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;

    [Header("AI Settings")]
    [SerializeField] private Transform[] wayPoint;
    [SerializeField] private NavMeshAgent navMeshAgent;

    //크리처 상태
    private enum creatureState { Patrol, Pursuit, Attack, Idle };
    private creatureState currentState;
    private bool isAttacking;
    private int currentPatrolIndex = 0;
    private bool patrolling = true;
    private float idleTime = 1f;
    private float Timer = 0f;
    private float attackRange = 5f;
    private float detectionRange = 10f;
    private float damage = 10f;

    //크리처 애니메이션
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

    private void Creature2Patrol()
    {
        //루틴대로 걸어가지만 만약에 앞에 막혀있으면 반대로 첫번째 지점으로 돌아가기

        if (wayPoint.Length == 0) return;

        navMeshAgent.isStopped = false;
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalking", true);

        navMeshAgent.destination = wayPoint[currentPatrolIndex].position;

        //순찰 지점에 도착했을 때 Idle상태로 전한
        if (navMeshAgent.remainingDistance < 0.5f && !navMeshAgent.pathPending)
        {
            currentState = creatureState.Idle;
        }
    }

    private void TheNextWayPoint()
    {
        if (patrolling)
        {
            ++currentPatrolIndex;
            if (currentPatrolIndex >= wayPoint.Length)
            {
                currentPatrolIndex = wayPoint.Length - 2;
                patrolling = false;
            }
        }
        else
        {
            --currentPatrolIndex;
            if (currentPatrolIndex < 0)
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
        player.StartCoroutine(player.PlayerHitEffect());
        if(itemmanager.currentItem != null)
        {
            itemmanager.DropCurrentItem();
        }
        //player.PlayerHitEffect();
        //Invoke("player.PlayerHitEffectEnd", 0.5f);
        camShake.StartCoroutine(camShake.Shake(0.2f, 0.3f));

        Vector3 dir = (player.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

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

        if (distanceToPlayer <= attackRange)
        {
            player.TakeDamage(damage);
            Debug.Log("Damage");
            Creature2Attack();
        }
        else
        {
            if (distanceToPlayer <= detectionRange)
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
        if (Timer >= idleTime)
        {
            Timer = 0f;
            TheNextWayPoint();
        }

    }

}


