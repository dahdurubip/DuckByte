using UnityEngine;
using System.Collections;

public class Creature2Clone : MonoBehaviour
{

    [Header("Scripts Settings")]
    [SerializeField] private ItemManager itemmanager;
    [SerializeField] private Player player;
    [SerializeField] private CameraMovement camShake;

    [Header("Default Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private AudioClip shutClip;

    private Transform playerTransform;
    private Vector3 targetPosition;
    private Animator animator;
    private bool isAttacking = false;
    private AudioSource audioSource;


    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialize(Vector3 eyePos, Transform player)
    {
        playerTransform = player;

        float targetPosY = playerTransform.position.y;
        targetPosition = eyePos;
        targetPosition.y = targetPosY;
        transform.position = targetPosition;
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

        player.TakeDamage(5);
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
