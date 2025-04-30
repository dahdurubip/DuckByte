using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Camera cam;
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;

    public float speed = 5f;
    public float runSpeed = 10f;
    public float crouchSpeed = 2.5f;      // crouch 상태의 이동 속도
    public float finalSpeed;
    public bool run;
    public bool toggleCameraRotation;
    public bool isCrouching;            // crouch 상태 여부

    public float smoothness = 10f;

    // crouch 상태 시 조정되는 캐릭터 콜라이더 값들
    public float crouchHeight = 1f;     // crouch 상태일 때의 height
    public float crouchCenterY = 0.7f;    // crouch 상태일 때의 center Y값
    public float crouchCapHeight = 1f;
    public float crouchCapCenterY = 0.5f;

    //중력
    public float gravity = -9.81f;
    private float verticalVelocity = 0f;

    private float originalHeight;       // 원래 height 저장
    private Vector3 originalCenter;     // 원래 center 저장
    private float originalCapHeight;       // 원래 height 저장
    private Vector3 originalCapCenter;     // 원래 center 저장

    private void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        originalHeight = characterController.height;   // 원래 height 저장
        originalCenter = characterController.center;     // 원래 center 저장
        originalCapHeight = capsuleCollider.height;
        originalCapCenter = capsuleCollider.center;
    }

    private void Update()
    {
        //중력 처리
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            //지면에 있으면 약간의 하강력을 유지하여 Controller가 튀는 것을 방지
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;

        // 카메라 회전 토글 (LeftAlt)
        toggleCameraRotation = Input.GetKey(KeyCode.LeftAlt);

        // 달리기 입력 (LeftShift)
        run = Input.GetKey(KeyCode.LeftShift);

        // ===== crouch 토글 추가 부분 시작 =====
        // C키를 한 번 누르면 crouch 상태 토글 (한 번 누르면 true, 다시 누르면 false)
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
        }
        // ===== crouch 토글 추가 부분 끝 =====

        // ===== 공격 추가 부분 시작 =====
        // F키를 누르면 attack 애니메이션(이름은 "attack") 실행
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("attack");
        }
        // ===== 공격 추가 부분 끝 =====

        InputMovement();
    }

    private void LateUpdate()
    {
        if (!toggleCameraRotation)
        {
            Vector3 playerRotate = Vector3.Scale(cam.transform.forward, new Vector3(1f, 0f, 1f));
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * smoothness);
        }
    }

    private void InputMovement()
    {
        if (isCrouching)
        {
            finalSpeed = crouchSpeed;
            characterController.height = crouchHeight;
            capsuleCollider.height = crouchCapHeight;
            // crouch 상태일 때 center의 Y값을 crouchCenterY로 설정
            characterController.center = new Vector3(originalCenter.x, crouchCenterY, originalCenter.z);
            capsuleCollider.center = new Vector3(originalCapCenter.x, crouchCapCenterY, originalCapCenter.z);
        }
        else
        {
            finalSpeed = (run) ? runSpeed : speed;
            characterController.height = originalHeight;
            capsuleCollider.height = originalCapHeight;
            characterController.center = originalCenter;
            capsuleCollider.center = originalCapCenter;
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        Vector3 moveDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");
        characterController.Move(moveDirection.normalized * (finalSpeed * Time.deltaTime));

        float percent = ((run) ? 1f : 0.5f) * moveDirection.magnitude;
        animator.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
        animator.SetBool("isCrouching", isCrouching);
    }
}
