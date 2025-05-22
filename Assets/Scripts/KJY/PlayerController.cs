using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 이동 속도 및 달리기 관련
    public float moveSpeed = 5f;             // 기본 이동 속도
    public float runSpeedMultiplier = 1.5f;    // 달리기 시 이동 속도 배수
    public float runBlendMultiplier = 2f;      // 달리기 시 Animator Vertical 값 배수 (걷기: 1, 달리기: 2)
    public float turnSpeed = 150f;             // 제자리 회전 속도 (도/초)

    // 중력 관련
    public float gravity = -9.81f;             // 중력 값
    private float verticalVelocity = 0f;       // 수직 속도

    // 점프 관련
    public float jumpForce = 7f;               // 점프 힘

    // crouch(앉기) 관련
    public float crouchSpeedMultiplier = 0.5f; // 앉은 상태 시 이동 속도 배수

    private CharacterController controller;
    private Animator animator;

    // 상태 플래그들
    //private bool isWalking = false;
    //private bool isTurning = false;

    private void Start()
    {
        controller = GetComponent<CharacterController>();    // CharacterController 컴포넌트 할당
        animator = GetComponentInChildren<Animator>();         // 자식 Animator 컴포넌트 할당
    }

    private void Update()
    {
        // ========= 중력 처리 =========
        bool isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            // 지면에 있을 경우, 약간의 하강력을 유지하여 Controller가 튀는 것을 방지
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;  // 중력 누적 적용


        // ========= 입력 값 받기 =========
        float inputVertical = Input.GetAxis("Vertical");       // 전/후진 입력 값
        float inputHorizontal = Input.GetAxis("Horizontal");   // 좌우 입력 값

        // ========= 앉기(Crouch) 입력 처리 =========
        bool isCrouching = Input.GetKey(KeyCode.C);      // C 키로 앉기 입력
        animator.SetBool("isCrouching", isCrouching);      // Animator에 isCrouching 값 전달

        // ========= 점프 처리 (앉은 상태에서는 점프 무시) =========
        bool jumpInput = false;
        if (!isCrouching)
        {
            jumpInput = Input.GetKeyDown(KeyCode.Space); // Space 키로 점프 입력 (단발성)
        }
        animator.SetBool("jump", jumpInput);              // Animator에 점프 값 전달
        if (jumpInput && isGrounded)
        {
            verticalVelocity = jumpForce;                 // 점프 시 수직 속도 설정
        }

        //알 깨기
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Attack");
        }

        // ========= 달리기 입력 처리 =========
        bool isRunning = Input.GetKey(KeyCode.LeftShift);   // 왼쪽 Shift 키로 달리기 입력
        animator.SetBool("isRunning", isRunning);           // Animator에 isRunning 값 전달

        // ========= 입력 체크 =========
        bool hasVerticalInput = Mathf.Abs(inputVertical) >= 0.1f;      // 전/후진 입력이 존재하는지 확인
        bool hasHorizontalInput = Mathf.Abs(inputHorizontal) >= 0.1f;  // 좌우 입력이 존재하는지 확인

        Vector3 movement = Vector3.zero;       // 최종 이동 벡터 초기화

        // ========= crouch 상태 처리 =========
        if (isCrouching)
        {
            if (hasVerticalInput || hasHorizontalInput)
            {
                // crouch 상태에서 이동 입력이 있을 때
                //isWalking = true;
                animator.SetBool("isWalking", true);  // 걷기 애니메이션 활성화
                animator.SetBool("isTurning", false);   // 회전은 비활성화
                animator.SetFloat("turn", 0f);          // 회전 파라미터 초기화

                // 이동 방향: 플레이어의 forward 및 right 벡터의 조합 (로컬 좌표 기준)
                Vector3 moveDir = (transform.forward * inputVertical + transform.right * inputHorizontal).normalized;
                // crouch 상태에서는 이동 속도 낮춤
                float effectiveSpeed = moveSpeed * crouchSpeedMultiplier;
                movement = moveDir * effectiveSpeed;

                // Animator에 Blend Tree 값 업데이트
                animator.SetFloat("Vertical", inputVertical);
                animator.SetFloat("Horizontal", inputHorizontal);
            }
            else
            {
                // crouch 상태에서 아무 입력이 없을 때
                animator.SetBool("isWalking", false);
                animator.SetBool("isTurning", false);
                animator.SetFloat("turn", 0f);
                animator.SetFloat("Vertical", 0f);
                animator.SetFloat("Horizontal", 0f);
            }
        }
        // ========= 일반 상태 (crouch 아님) 처리 =========
        else
        {
            if (hasVerticalInput)
            {
                // 전/후진 입력이 있을 때 (걷기 또는 달리기)
                //isWalking = true;
                animator.SetBool("isWalking", true);  // 걷기 애니메이션 활성화
                animator.SetBool("isTurning", false);   // 회전 애니메이션 비활성화
                animator.SetFloat("turn", 0f);          // 회전 파라미터 초기화

                // 이동 방향 계산: 전진/후진 및 좌우 입력 모두 반영
                Vector3 moveDir = (transform.forward * inputVertical + transform.right * inputHorizontal).normalized;
                // 달리기 시 속도에 배수 적용
                float effectiveSpeed = moveSpeed * (isRunning ? runSpeedMultiplier : 1f);
                movement = moveDir * effectiveSpeed;

                // Animator에 Blend Tree 파라미터 업데이트
                // 달리기 모드이면 Vertical 값에 runBlendMultiplier 곱해서 전달
                float verticalAnim = inputVertical * (isRunning ? runBlendMultiplier : 1f);
                animator.SetFloat("Vertical", verticalAnim);
                animator.SetFloat("Horizontal", inputHorizontal);
            }
            // 전진 입력 없이 좌우 입력만 있을 때 → 제자리 회전 처리
            else if (!hasVerticalInput && hasHorizontalInput)
            {
                //isTurning = true;
                animator.SetBool("isTurning", true);  // 회전 애니메이션 활성화
                animator.SetFloat("turn", inputHorizontal); // 회전 정도 전달

                // 좌우 입력에 따라 플레이어 제자리 회전
                float rotationAmount = inputHorizontal * turnSpeed * Time.deltaTime;
                transform.Rotate(0, rotationAmount, 0);

                animator.SetFloat("Vertical", 0f);
                animator.SetFloat("Horizontal", 0f);
            }
            else
            {
                // 입력이 없으면 Idle 상태
                animator.SetBool("isWalking", false);
                animator.SetBool("isTurning", false);
                animator.SetFloat("turn", 0f);
                animator.SetFloat("Vertical", 0f);
                animator.SetFloat("Horizontal", 0f);
            }
        }

        // ========= 최종 이동 적용 =========
        Vector3 finalMove = movement;
        finalMove.y = verticalVelocity;                      // 수직 속도 적용 (중력, 점프 포함)
        controller.Move(finalMove * Time.deltaTime);         // CharacterController로 이동
    }
}
