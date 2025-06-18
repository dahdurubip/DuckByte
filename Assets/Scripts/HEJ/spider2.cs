using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spider2 : MonoBehaviour
{

    public Transform[] waypoints;
    private int _currentWaypoint = 0;
    public float detectionRadius = 10f;
    public float attackRange = 2f;

    private NavMeshAgent _agent;
    private Animator _anim;
    private Transform _player;

    private enum State { Patrol, Chase, Attack }
    private State _state = State.Patrol;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponent<Animator>();
        _player = GameObject.FindWithTag("Player")?.transform;
        if (_player == null)
            Debug.LogError("Player 태그를 찾을 수 없습니다.");
    }

    private void Start()
    {
        GoToNextWaypoint();
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Patrol:
                PatrolUpdate();
                break;
            case State.Chase:
                ChaseUpdate();
                break;
            case State.Attack:
                AttackUpdate();
                break;
        }

        float distToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distToPlayer <= detectionRadius && _state == State.Patrol)
        {
            _state = State.Chase;
        }
        // 플레이어가 범위 밖으로 나가면 다시 순찰 모드
        else if (distToPlayer > detectionRadius && _state != State.Patrol)
        {
            _state = State.Patrol;
            GoToNextWaypoint();
        }
    }

    private void PatrolUpdate()
    {
    
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            GoToNextWaypoint();
        }

  
        _anim.SetBool("isWalk", true);
        _anim.SetBool("isAttack", false);
    }

    private void ChaseUpdate()
    {
        // 플레이어 위치로 계속 이동
        _agent.SetDestination(_player.position);


        if (_agent.remainingDistance <= attackRange)
        {
            _state = State.Attack;
        }
        else
        {
            _anim.SetBool("isWalk", true);
            _anim.SetBool("isAttack", false);
        }
    }

    private void AttackUpdate()
    {
        // 멈춰서 공격 애니메이션
        _agent.ResetPath();
        _anim.SetBool("isWalk", false);
        _anim.SetBool("isAttack", true);


    }

    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_currentWaypoint].position);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
