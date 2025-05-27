using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float CurrentHp
    {
        get => currentHp;
        set
        {
            currentHp = Mathf.Clamp(value, 0f, maxHp);
            //hpBar.UpdateHpBar(maxHp, currentHp);

            if (currentHp <= 0f)
            {
                Die();
            }
        }
    }


    [Header("Hp Settings")]
    [SerializeField] private float maxHp = 100f;

    [Header("Shake Settings")]
    [SerializeField] private GameObject hitEffect1;
    [SerializeField] private GameObject hitEffect2;
    [SerializeField] private CameraMovement cam;

    [SerializeField] private SceneLoader sceneLoader;
    private float currentHp;
    private Animator animator;


    private void Start()
    {
        animator = GetComponent<Animator>();
        //체력 초기화 및 HP 바 초기화
        CurrentHp = maxHp;  
    }

    //private void Update()
    //{
    //    Debug.Log("CurHP" + CurrentHp);
    //    Debug.Log("curHP" + currentHp);
    //}

    public void TakeDamage(float damage)
    {
        CurrentHp -= damage;
    }

    //피격효과
    public IEnumerator PlayerHitEffect()
    {
        animator.SetBool("hit", true);
        hitEffect1.SetActive(true);
        hitEffect2.SetActive(true);
        //일정시간 대기
        yield return new WaitForSeconds(0.8f);
        //비활성화
        animator.SetBool("hit", false);
        hitEffect1.SetActive(false);
        hitEffect2.SetActive(false);
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");
        StopAllCoroutines();
        //여기서 사망 애니메이션, 게임 오버 처리 등 넣기
        animator.SetBool("die", true);
        //게임오버 화면 전환
        sceneLoader.RestartScene();
    }

    public void Heal(float amount)
    {
        CurrentHp += amount;
    }
}
