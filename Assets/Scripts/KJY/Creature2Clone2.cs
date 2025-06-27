using UnityEngine;
using System.Collections;

public class Creature2Clone2 : MonoBehaviour
{

    [Header("Scripts Settings")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;

    [Header("Default Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float spawnDistance = 2f;
    [SerializeField] private AudioClip shutClip;

    private Transform playerTransform;
    private Animator animator;
    private bool isAttacking = false;
    private AudioSource audioSource;


    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Transform player)
    {
        playerTransform = player;

        Vector3 spawnPosition = playerTransform.position + playerTransform.forward * spawnDistance;

        spawnPosition.y = playerTransform.position.y; // Y축 위치를 플레이어와 동일하게 설정

        transform.position = spawnPosition; 

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        // Y축을 0으로 설정하여 수평 방향만
        directionToPlayer.y = 0; 
        transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist > attackRange)
        {
            Chase();
        }
        else
        {
            //공격
            if (!isAttacking)
            {
                Attack();
            }

        }
    }

    private void Chase()
    {
        //추적
        Vector3 dir = (playerTransform.position - transform.position);
        dir.y = 0;
        dir.Normalize();

        //회전
        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

        //이동
        transform.position += dir * moveSpeed * Time.deltaTime;

        //애니메이션
        animator.SetBool("run", true);
        animator.ResetTrigger("attack");
        isAttacking = false;
    }

    private void Attack()
    {
        isAttacking = true;
        animator.SetBool("run", false);
        animator.SetTrigger("attack");
        audioSource.Stop();
        audioSource.PlayOneShot(shutClip);
    }

    public void ApplyAttackDamage()
    {
        if (player == null) return;

        player.TakeDamage(30);
        //Debug.Log("때리는중");
        player.StartCoroutine(player.PlayerHitEffect());

        //손에 아이템 있으면 호출
        if (itemmanager.currentItem != null && !itemmanager.currentItem.CompareTag("Flashlight"))
        {
            itemmanager.DropCurrentItem();
        }

        camShake.StartCoroutine(camShake.Shake(0.2f, 0.3f));
    }

    public void EndAttack()
    {
        isAttacking = false;
    }
}
