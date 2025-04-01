using GLTFast.Schema;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class move : MonoBehaviour
{

    [SerializeField] private NavMeshAgent navMeshAgent;
    private Animator animator;
    [SerializeField] private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        navMeshAgent.SetDestination(player.transform.position);
        //navMeshAgent.isStopped = false;
        animator.SetBool("isWalking", true);
        
    }
}
//{
//    [SerializeField]
//    private float moveSpeed = 5.0f;
//    private NavMeshAgent navMeshAgent;
//    [SerializeField] private Transform waypoint;

//    private void Awake()
//    {
//        navMeshAgent = GetComponent<NavMeshAgent>();

//    }

//    private void Update()
//    {
//        Vector3 WayP = waypoint.position;
//        MoveTo(WayP);
//    }

//    public void MoveTo(Vector3 goalPosition)
//    {
//        // 기존에 이동 행동을 하고 있었다면 코루틴 중지
//        StopCoroutine("OnMove");
//        // 이동 속도 설정
//        navMeshAgent.speed = moveSpeed;
//        // 목표지점 설정 (목표까지의 경로 계산 후 알아서 움직인다)
//        navMeshAgent.SetDestination(goalPosition);
//        // 이동 행동에 대한 코루틴 시작
//        StartCoroutine("OnMove");
//    }

//    IEnumerator OnMove()
//    {
//        while (true)
//        {
//            // 목표 위치(navMeshAgent.destination)와 내 위치(transform.position)의 거리가 0.1미만일 때
//            // 즉, 목표 위치에 거의 도착했을 때
//            if (Vector3.Distance(navMeshAgent.destination, transform.position) < 0.1f)
//            {
//                // 내 위치를 목표 위치로 설정
//                transform.position = navMeshAgent.destination;
//                // SetDestination()에 의해 설정된 경로를 초기화. 이동을 멈춘다
//                navMeshAgent.ResetPath();

//                break;
//            }

//            yield return null;
//        }
//    }
//}
