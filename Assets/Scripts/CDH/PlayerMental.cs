using UnityEngine;
using UnityEngine.SceneManagement; // 죽었을 때 씬 리로드용
using System.Collections;
using UnityEngine.Audio;

public class PlayerMental : MonoBehaviour
{
    [Header("정신력 설정")]
    public float maxMental = 100;
    public float currentMental;

    [Header("회복 설정")]
    public float recoveryRate = 5f; // 초당 회복량
    public float recoveryRange = 3f; // 제단과의 거리

    // 현재 조작 반전상태인지를 나타내줌
    [Header("상태")]
    public bool isReversingControl = false;

    // 제단 오브젝트의 위치를 저장
    //public Transform altar;
    //private PlayerMovement playerMovement; // 조작 반전 적용용

    private Coroutine recoverCoroutine;
    public bool isHealing { get; private set; }
    public bool isInSlowZone { get; private set; }
    //public bool IsReversingControl { get; private set; }

    public PlayerMovement pm;
    public ParticleSystem debuffEffect;
    //[SerializeField] AudioSource debuffAudio;

    public AudioClip heal; // 1번 구역 음악
    public AudioClip debuff; // 2번 구역 음악

    [SerializeField] AudioSource playAudio;
    // 멘탈 무너졌을때 효과
    [SerializeField] GameObject mentalVolume;


    void Start()
    {
        currentMental = maxMental;

        debuffEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        //altar = GameObject.FindGameObjectWithTag("Altar")?.transform;

        //playerMovement = GetComponent<PlayerMovement>();
        //if (playerMovement == null)
        //{
        //    Debug.LogWarning("PlayerMovement 컴포넌트를 찾을 수 없습니다.");
        //}
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = true;
            // 이동 속도 줄이는 코드넣기
            pm.speed = 2f;
        }

        if (other.CompareTag("HealZone"))
        {
           // Debug.Log("힐존 들어옴");

            //healAudio.Play();
            playAudio.loop = true;
            PlaySound(heal);
            if (recoverCoroutine == null)
            {
                isHealing = true;
                recoverCoroutine = StartCoroutine(RecoverMental());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = false;
            // 이동 속도 원상복구하는 코드넣기
            pm.speed = 5f;
        }

        if (other.CompareTag("HealZone") && recoverCoroutine != null)
        {
            //Debug.Log("힐존 나감");
            //healAudio.Stop();
            playAudio.Stop();
            isHealing = false;
            StopCoroutine(recoverCoroutine);
            recoverCoroutine = null;
        }
    }

    IEnumerator RecoverMental()
    {
        //Debug.Log("회복 시작");
        while (true)
        {
            currentMental += recoveryRate * Time.deltaTime;
            currentMental = Mathf.Clamp(currentMental, 0f, maxMental);

            if (currentMental > 50 && isReversingControl)
            {
                isReversingControl = false;
                //Debug.Log("정신력 회복 → 조작 정상화");
                mentalVolume.SetActive(false);
            }

            yield return null; // 다음 프레임까지 대기
        }
    }


    void Update()
    {
        // 정신력 회복 조건
        //if (altar != null)
        //{
        //    // 플레이어의 위치와 제단의 위치사이의 거리를 구해 회복 구역 이내라면 회복함수를 호출
        //    float distanceToAltar = Vector3.Distance(transform.position, altar.position);
        //    if (distanceToAltar <= recoveryRange)
        //    {
        //        RecoverMental();
        //    }
        //}

        //if (isInSlowZone)
        //{
        //    pm.finalSpeed = 2f;
        //}

        //Debug.Log(currentMental);
        //Debug.Log(pm.finalSpeed);

        // 정신력 상태에 따른 효과
        // 현재 정신력이 30이하이면서 조작반전이 되지않았다면 효과함수 호출
        if (currentMental <= 50 && !isReversingControl)
        {
            TriggerLowMentalEffects();
        }

        // 정신력이 0이하가되면 사망함수 호출
        if (currentMental <= 0)
        {
            //Die();
        }
    }

    // 외부에서 정신력을 깎을때 사용 할 함수
    public void TakeMentalDamage(int amount)
    {
        currentMental -= amount;
        //Clamp를 통해서 값을 0~100사이로 유지
        currentMental = Mathf.Clamp(currentMental, 0, maxMental);
        // 정신력 감소 파티클 재생 및 오디오 재생
        debuffEffect.Play();
        //debuffAudio.Play();
        playAudio.loop = false;
        PlaySound(debuff);

       // Debug.Log("정신력 감소: " + currentMental);
    }

    // 회복 함수
    //void RecoverMental()
    //{
    //    Debug.Log("회복중");
    //    // 정신력을 초당 회복
    //    currentMental += recoveryRate * Time.deltaTime;
    //    currentMental = Mathf.Clamp(currentMental, 0f, maxMental);

    //    // 조작 반전 복구
    //    // 정신력이 30이상이면서 조작반전상태일시에는 조작반전 상태를 해제
    //    if (currentMental > 30 && isReversingControl)
    //    {
    //        isReversingControl = false;
    //        //if (playerMovement != null)
    //        //    playerMovement.SetReverseControl(false);

    //        Debug.Log("정신력 회복 → 조작 정상화");
    //    }
    //}

    // 낮은 정신력 효과
    void TriggerLowMentalEffects()
    {
        // 조작반전 시작
        isReversingControl = true;

        //if (playerMovement != null)
        //    playerMovement.SetReverseControl(true);

        // 화면 왜곡 효과도 여기서 시작
        mentalVolume.SetActive(true);
        //Debug.Log("정신력 낮음! 화면 왜곡 + 조작 반전 시작");
    }

    // 죽음 함수
    void Die()
    {
        Debug.Log("정신력 0 → 사망!");
        // 여기서 게임 오버 처리 (추후 체크포인트 만들어서 컷씬 끝난 후로 되돌릴 예정)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void PlaySound(AudioClip newClip)
    {
        //if (audioSource.clip == newClip && audioSource.isPlaying)
        //    return; // 이미 재생 중이면 무시

        playAudio.clip = newClip;
        playAudio.Play();
    }
}
