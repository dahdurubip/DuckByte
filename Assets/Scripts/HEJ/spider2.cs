using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class Spider2 : MonoBehaviour
{
    [Header("웨이포인트들")]
    public Transform[] waypoints;

    [Header("탐지/공격 거리")]
    public float detectionRadius = 10f;
    public float attackRange = 2f;

    [Header("공격 쿨다운(초)")]
    public float attackCooldown = 1.5f;

    [Header("공격 애니 길이(초)")]
    [Tooltip("Animator에서 Attack 클립 재생 길이에 맞춰 주세요.")]
    public float attackAnimDuration = 0.8f;

    private NavMeshAgent _agent;
    private Animator _anim;
    private Transform _player;

    private int _currentWaypoint = 0;
    private float _nextAttackTime = 0f;
    private bool _isAttacking = false;

    private enum State { Patrol, Chase, Attack }
    private State _state = State.Patrol;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        if (_anim == null)
            Debug.LogError($"{name}: 자식에 Animator가 없습니다.");

        var go = GameObject.FindWithTag("Player");
        if (go != null) _player = go.transform;
        else Debug.LogError($"{name}: Player 태그가 없습니다.");
    }

    void Start()
    {
        GoToNextWaypoint();
    }

    void Update()
    {
        if (_player == null) return;

        float d = Vector3.Distance(transform.position, _player.position);

        // Patrol ↔ Chase 전환
        if (d <= detectionRadius && _state == State.Patrol)
            _state = State.Chase;
        else if (d > detectionRadius && _state != State.Patrol)
        {
            _state = State.Patrol;
            GoToNextWaypoint();
        }

        // 상태별 처리
        switch (_state)
        {
            case State.Patrol: PatrolUpdate(); break;
            case State.Chase: ChaseUpdate(d); break;
            case State.Attack: AttackUpdate(d); break;
        }
    }

    void PatrolUpdate()
    {
        // 자동 순찰
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _currentWaypoint = (_currentWaypoint + 1) % waypoints.Length;
            GoToNextWaypoint();
        }

        _anim?.SetBool("isWalk", true);
        _anim?.SetBool("isAttack", false);
    }

    void ChaseUpdate(float d)
    {
        _agent.SetDestination(_player.position);
        _anim?.SetBool("isWalk", true);
        _anim?.SetBool("isAttack", false);

        if (d <= attackRange)
            _state = State.Attack;
    }

    void AttackUpdate(float d)
    {
        // 멈추기
        _agent.ResetPath();
        _anim?.SetBool("isWalk", false);

        // 사거리 안이고, 아직 쿨다운 끝났고, 코루틴 중이 아니면 공격 시작
        if (d <= attackRange && !_isAttacking && Time.time >= _nextAttackTime)
        {
            StartCoroutine(DoAttack());
        }
        // 사거리 벗어나면 다시 Chase
        else if (d > attackRange)
        {
            _state = State.Chase;
        }
    }

    IEnumerator DoAttack()
    {
        _isAttacking = true;
        _anim?.SetBool("isAttack", true);

        // 애니메이션 지속 시간 만큼 대기
        yield return new WaitForSeconds(attackAnimDuration);

        // 공격 마무리
        _anim?.SetBool("isAttack", false);
        _isAttacking = false;
        _nextAttackTime = Time.time + attackCooldown;
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        _agent.SetDestination(waypoints[_currentWaypoint].position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
