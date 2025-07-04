using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Camera cam;
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;
    private PlayerMental mental;

    [Header("Player Default Settings")]
    //걷기 속도
    public float speed = 5f;
    //현재 프레임의 최종 속도
    public float finalSpeed;
    //현재 달리기 상태인지 여부 (InputMovement에서 사용)
    public bool run;
    //달리기 속도
    [SerializeField] private float runSpeed = 10f;
    //앉기 속도
    [SerializeField] private float crouchSpeed = 2.5f;
    //카메라 회전 토글 여부
    [SerializeField] private bool toggleCameraRotation;
    //앉기 상태 여부
    [SerializeField] public bool isCrouching;
    private float crouchCooldownTimer = 0f;
    [SerializeField] private bool canCrouch = true;


    [Header("Crouch Settings")]
    //앉았을 때 CharacterController 높이
    [SerializeField] private float crouchHeight = 1f;
    //앉았을 때 CharacterController 중심 Y값
    [SerializeField] private float crouchCenterY = 0.7f;
    //앉았을 때 CapsuleCollider 높이
    [SerializeField] private float crouchCapHeight = 1f;
    //앉았을 때 CapsuleCollider 중심 Y값
    [SerializeField] private float crouchCapCenterY = 0.5f;
    public bool playerCrouch;
    //앉기 지속 시간 타이머
    private float crouchTimer = 0f;

    [Header("Physics Settings")]
    //중력 값
    [SerializeField] private float gravity = -10f;
    //수직 속도
    private float verticalVelocity = 0f;

    //Character/Capsule Collider 원래 값 저장용 변수
    private float originalHeight;
    private Vector3 originalCenter;
    private float originalCapHeight;
    private Vector3 originalCapCenter;

    //최종 이동 방향
    private Vector3 moveDirection;

    [Header("Stamina Settings")]
    //초기 스태미나
    [SerializeField] private float initialStamina = 100f;
    //현재 스태미나
    private float currentStamina;
    //최대 스태미나
    private float maxStamina;
    //스태미나 표시 UI
    [SerializeField] TMP_Text stamina_UI;
    [SerializeField] private float staminaConsumeRate = 10f; //<<< 초당 소모량
    [SerializeField] private float staminaRegenRate = 3f;  //<<< 초당 회복량
    [SerializeField] private float exhaustionDuration = 3f;

    [Header("AudioClip")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;
    [SerializeField] private AudioClip breatheClip;
    [SerializeField] private AudioClip crouchingWalkClip;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource footstepAudioSource; // 발소리용 AudioSource
    [SerializeField] private AudioSource sfxAudioSource;      // 효과음용 AudioSource (숨소리 등)

    //플레이어 이동 가능 여부
    public bool isMovable = true;
    //회전 시 사용되는 속도
    private float rotationVelocity;
    //회전 부드러움 정도
    public float rotationSmoothTime = 0.1f;
    private bool isExhausted = false;

    public bool IsMoving { get; private set; }
    public GameManager GM;


    private void Awake()
    {
        cam = Camera.main;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        mental = GetComponent<PlayerMental>();
    }

    private void Start()
    {
        //발소리 오디오 소스 설정
        if (footstepAudioSource != null)
        {
            footstepAudioSource.Stop();
            footstepAudioSource.loop = true;
        }
        //효과음 오디오 소스 설정 (루프 해제)
        if (sfxAudioSource != null)
        {
            sfxAudioSource.loop = false;
        }
        //CharacterController 원래 값 저장
        originalHeight = characterController.height;
        originalCenter = characterController.center;
        //CapsuleCollider 원래 값 저장
        originalCapHeight = capsuleCollider.height;
        originalCapCenter = capsuleCollider.center;

        //스태미나 초기화
        currentStamina = initialStamina;
        maxStamina = initialStamina;
        UpdateStaminaUI();

        //CharacterController 설정
        characterController.stepOffset = 0.7f; //넘을 수 있는 턱의 최대 높이
        characterController.slopeLimit = 50f;  //오를 수 있는 최대 경사 각도
    }

    private void Update()
    {
        HandleGravity();
        HandleCameraToggle();

        if (isMovable) //플레이어가 움직일 수 있는 상태일 때만 아래 로직 처리
        {
            HandleStaminaAndRunState(); //스태미나 관리 및 달리기 상태 결정
            InputMovement(); //실제 이동 처리
            HandleStaminaRegeneration(); //스태미나 회복 처리
            UpdateStaminaUI(); //스태미나 UI 업데이트
        }

        HandleCrouch();
        HandleAudio();

        // 쿨타임 타이머 감소
        if (!canCrouch)
        {
            //Debug.Log("앉기 타이머 감소중");
            crouchCooldownTimer -= Time.deltaTime;
            if (crouchCooldownTimer <= 0f)
            {
                //Debug.Log(crouchCooldownTimer);
                canCrouch = true;
            }
        }
    }

    //중력 처리
    private void HandleGravity()
    {
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            //지면에 있을 때 약간의 하강력을 유지하여 CharacterController가 튀는 것을 방지
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime; // 매 프레임 중력 적용
    }

    //카메라 회전 토글 처리
    private void HandleCameraToggle()
    {
        toggleCameraRotation = Input.GetKey(KeyCode.LeftAlt);
    }

    //스태미나 관리 및 달리기 상태 결정
    private void HandleStaminaAndRunState()
    {
        //이미 탈진 상태이거나 움직일 수 없는 상태이면 아무것도 하지 않음
        if (isExhausted || !isMovable) return;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);
        this.run = false;

        if (wantsToRun && !isCrouching)
        {
            if (currentStamina > 0)
            {
                this.run = true;
                float previousStamina = currentStamina;
                //Time.deltaTime을 곱해 초당 소모량으로 변경
                currentStamina -= staminaConsumeRate * Time.deltaTime;
                if (currentStamina < 0) currentStamina = 0;

                //스태미나가 방금 0이 되었다면 탈진 코루틴 시작
                if (previousStamina > 0 && currentStamina == 0)
                {
                    StartCoroutine(ExhaustionCoroutine());
                }
            }
            else
            {
                this.run = false;
            }
        }
    }

    //스태미나 회복 처리
    private void HandleStaminaRegeneration()
    {
        //달리고 있지 않고, 앉아있지 않으며, 탈진 상태가 아닐 때만 회복
        if (!this.run && !isCrouching && !isExhausted)
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina) currentStamina = maxStamina;
            }
        }
    }

    private IEnumerator ExhaustionCoroutine()
    {
        // 1. 탈진 상태 시작
        isExhausted = true;
        isMovable = false; // 움직임 비활성화
        this.run = false;

        // 이동 애니메이션 정지
        animator.SetFloat("Blend", 0f);

        // 2. 숨소리 재생
        if (sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(breatheClip);
        }

        if (footstepAudioSource != null)
        {
            footstepAudioSource.Stop();
        }
        // 3. 3초 동안 대기
        yield return new WaitForSeconds(exhaustionDuration);

        // 4. 탈진 상태 해제
        isMovable = true;  // 움직임 다시 활성화
        isExhausted = false;
    }


    //플레이어 입력 및 이동 처리
    private void InputMovement()
    {
        //앉기 상태에 따른 속도 및 콜라이더 크기 조절
        if (isCrouching)
        {
            finalSpeed = crouchSpeed;
            characterController.height = crouchHeight;
            capsuleCollider.height = crouchCapHeight;
            characterController.center = new Vector3(originalCenter.x, crouchCenterY, originalCenter.z);
            capsuleCollider.center = new Vector3(originalCapCenter.x, crouchCapCenterY, originalCapCenter.z);
        }
        else
        {
            //this.run은 HandleStaminaAndRunState()에서 스태미나를 고려하여 결정된 값
            finalSpeed = (this.run) ? runSpeed : speed;
            characterController.height = originalHeight;
            capsuleCollider.height = originalCapHeight;
            characterController.center = originalCenter;
            capsuleCollider.center = originalCapCenter;
        }

        //입력 값 받기
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //뒤로 이동 방지 (필요에 따라 유지 또는 제거)
        //if (vertical < 0)
        //    vertical = 0;

        //PlayerMental 컴포넌트에 따른 입력 반전 처리 (주석 처리된 "내일 이거 해보깅~!" 관련)
        if (mental != null && mental.isReversingControl)
        {
            vertical = -vertical;
            horizontal = -horizontal;
        }

        //입력 방향 벡터 계산 (카메라 기준 아님)
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (currentStamina <= 0 && run)
        {
            //스태미나가 0 이하라면 이동 입력을 무시 (달리기 시도 중일 때만)
            inputDirection = Vector3.zero;
        }


        //카메라 방향 기준으로 이동 방향 벡터 변환
        Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.transform.right;
        //moveDirection은 수평 이동만을 나타냄
        moveDirection = (camForward * inputDirection.z + camRight * inputDirection.x).normalized;

        //최종 이동 벡터 계산 (수평 이동 + 수직 속도(중력))
        Vector3 finalMoveVector = moveDirection * finalSpeed;
        finalMoveVector.y = verticalVelocity; // 중력 적용

        //CharacterController를 사용하여 이동
        characterController.Move(finalMoveVector * Time.deltaTime);

        //실제 움직임 여부 판단 (XZ 평면 기준)
        IsMoving = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude > 0.1f;

        //플레이어 회전 처리
        if (IsMoving && !toggleCameraRotation)
        {
            //입력 방향을 기준으로 목표 회전 값 계산
            if (inputDirection.sqrMagnitude > 0.01f) //입력이 있을 때만 회전
            {
                float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
        }


        float animatorSpeedPercent = 0f;
        if (IsMoving)
        {
            animatorSpeedPercent = (this.run && !isCrouching) ? 1f : 0.5f;
        }

        animator.SetFloat("Blend", animatorSpeedPercent * inputDirection.magnitude, 0.1f, Time.deltaTime);
        animator.SetBool("isCrouching", isCrouching);
    }

    //앉기 처리
    private void HandleCrouch()
    {
        // 쿨타임 중이면 무시
        if (!canCrouch) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            playerCrouch = isCrouching; //playerCrouch를 isCrouching과 동기화
            crouchTimer = 0f; //앉기/일어서기 시 타이머 초기화
        }

        //앉기 상태일 때 타이머 로직 (7초 후 강제 기상)
        if (isCrouching)
        {
            crouchTimer += Time.deltaTime;
            if (crouchTimer >= 7f)
            {
                isCrouching = false;
                playerCrouch = false;
                crouchTimer = 0f;

                // 강제 기상 후 쿨타임 시작
                canCrouch = false;
                crouchCooldownTimer = 5f; // 재앉기 불가

                if (!isExhausted)
                {
                    StartCoroutine(ExhaustionCoroutine());
                }
            }
        }

        
    }

    private void HandleAudio()
    {
        if (footstepAudioSource == null) return;

        if (IsMoving)
        {
            AudioClip targetClip = null;

            if (isCrouching)
            {
                targetClip = crouchingWalkClip;
            }
            else if (run)
            {
                targetClip = runClip;
            }
            else
            {
                targetClip = walkClip;
            }

            if (footstepAudioSource.clip != targetClip)
            {
                footstepAudioSource.clip = targetClip;
                footstepAudioSource.Play();
            }
            else if (!footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Play();
            }
        }
        else
        {
            if (footstepAudioSource.isPlaying)
            {
                footstepAudioSource.Stop();
            }
        }
    }

    //스태미나 UI 업데이트
    private void UpdateStaminaUI()
    {
        if (stamina_UI != null) // UI 요소가 할당되었는지 확인
        {
            stamina_UI.text = ((int)(currentStamina / maxStamina * 100f)).ToString() + "%";
            //stamina_UI.text = ((int)currentStamina).ToString() + " / " + ((int)maxStamina).ToString();
        }
        else
        {
            // Debug.LogWarning("Stamina UI is not assigned."); // 필요시 경고 로그
        }
    }

}
