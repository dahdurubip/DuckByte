using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class spider2 : MonoBehaviour
{
    private WSBItemManager ItemManager;

    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints; // 경로 배열 설정
    public Transform target;      // 타겟 설정

    private Animator animator;
    int m_CurrentWaypointIndex;   // 최근 경로 번호
    private Transform searchTarget = null;

    public AudioClip[] Spidershout = null;
    public AudioClip SpiderFoot;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // 태그로 플레이어 찾기
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            Debug.LogError("Player 태그 오브젝트를 찾을 수 없습니다.");

        searchTarget = this.GetComponent<Transform>();
    }

    private void Start()
    {
        if (waypoints != null && waypoints.Length > 0 && navMeshAgent != null)
        {
            navMeshAgent.SetDestination(waypoints[0].position); // 거미 시작점
            Debug.Log("check");
        }
        else
        {
            Debug.LogError("Waypoints 또는 navMeshAgent가 설정되지 않았습니다.");
        }
    }

    private void Update()
    {
        setDistance();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
            else
                Debug.LogError("Player 태그 오브젝트를 찾을 수 없습니다.");

            if (navMeshAgent != null && target != null && animator != null)
            {
                navMeshAgent.SetDestination(target.position);
                animator.SetBool("isWalk", false);
                animator.SetBool("isAttack", true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            target = null;
            if (animator != null)
            {
                animator.SetBool("isWalk", true);
                animator.SetBool("isAttack", false);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (navMeshAgent != null && target != null && animator != null)
            {
                navMeshAgent.SetDestination(target.position);
                animator.SetBool("isWalk", false);
                animator.SetBool("isAttack", true);
            }
        }
    }

    private void RunAway()
    {
        target = null;
        if (animator != null)
        {
            animator.SetBool("isWalk", true);
            animator.SetBool("isAttack", false);
        }
    }

    public void attack()
    {
        float damage = Random.Range(5f, 11f);
        // PlayerController.Damage(damage);
        // Debug.Log(PlayerController.CurHp);
    }

    private void setDistance()
    {
        if (navMeshAgent != null && waypoints != null && waypoints.Length > 0)
        {
            if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
            {
                m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
                navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
            }
        }
    }

    private void Spiderfoot()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && SpiderFoot != null)
        {
            audio.Stop();
            audio.PlayOneShot(SpiderFoot);
        }
    }

    private void attackSound()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && Spidershout != null && Spidershout.Length > 0)
        {
            AudioClip Spider = Spidershout[0];
            audio.Stop();
            audio.PlayOneShot(Spider);
        }
    }
}
