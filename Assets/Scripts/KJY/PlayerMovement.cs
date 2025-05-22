using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Camera cam;
    private CharacterController characterController;
    private CapsuleCollider capsuleCollider;

    public float speed = 5f;
    public float runSpeed = 10f;
    //crouch 상태의 이동 속도
    public float crouchSpeed = 2.5f;
    public float finalSpeed;
    public bool run;
    public bool toggleCameraRotation;
    //crouch 상태 여부
    public bool isCrouching;

    public float smoothness = 10f;

    //crouch 상태 시 조정되는 캐릭터 콜라이더 값들
    //crouch 상태일 때의 height
    public float crouchHeight = 1f;
    //crouch 상태일 때의 center Y값
    public float crouchCenterY = 0.7f;
    public float crouchCapHeight = 1f;
    public float crouchCapCenterY = 0.5f;
    public bool playerCrouch;
    private float Timer = 0f;

    //중력 -9.81f
    public float gravity = -10f;
    private float verticalVelocity = 0f;

    //원래 height 저장
    private float originalHeight;
    //원래 center 저장
    private Vector3 originalCenter;
    //원래 height 저장
    private float originalCapHeight;
    //원래 center 저장
    private Vector3 originalCapCenter;


    private float rotationVelocity;
    public float rotationSmoothTime = 0.1f;
    public bool IsMoving { get; private set; }


    private void Start()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        //원래 height 저장
        originalHeight = characterController.height;
        //원래 center 저장
        originalCenter = characterController.center;
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

        //카메라 회전 토글 (LeftAlt)
        toggleCameraRotation = Input.GetKey(KeyCode.LeftAlt);

        //달리기 입력 (LeftShift)
        run = Input.GetKey(KeyCode.LeftShift);


        //C키를 한 번 누르면 crouch 상태 토글 (한 번 누르면 true, 다시 누르면 false)
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            playerCrouch = !playerCrouch;
        }





        //스페이스키 공격
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("attack");
        }


        InputMovement();
    }

    //private void LateUpdate()
    //{
        //if (!toggleCameraRotation)
        //{
        //    Vector3 playerRotate = Vector3.Scale(cam.transform.forward, new Vector3(1f, 0f, 1f));
        //    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(playerRotate), Time.deltaTime * smoothness);
        //}
    //}

    //    private void InputMovement()
    //    {
    //        if (isCrouching)
    //        {
    //            finalSpeed = crouchSpeed;
    //            characterController.height = crouchHeight;
    //            capsuleCollider.height = crouchCapHeight;

    //            //crouch 상태일 때 center의 Y값을 crouchCenterY로 설정
    //            characterController.center = new Vector3(originalCenter.x, crouchCenterY, originalCenter.z);
    //            capsuleCollider.center = new Vector3(originalCapCenter.x, crouchCapCenterY, originalCapCenter.z);

    //            Timer += Time.deltaTime;
    //            //Debug.Log("Timer : " + Timer);
    //            if (Timer >= 7f)
    //            {
    //                isCrouching = false;
    //                playerCrouch = false;
    //                Timer = 0f;
    //            }
    //        }
    //        else
    //        {
    //            finalSpeed = (run) ? runSpeed : speed;
    //            characterController.height = originalHeight;
    //            capsuleCollider.height = originalCapHeight;
    //            characterController.center = originalCenter;
    //            capsuleCollider.center = originalCapCenter;
    //        }

    //        //이때 조작 반전 if문 넣어보기
    //        Vector3 forward = transform.TransformDirection(Vector3.forward);
    //        Vector3 right = transform.TransformDirection(Vector3.right);

    //        float horizontal = Input.GetAxisRaw("Horizontal");
    //        float vertical = Input.GetAxisRaw("Vertical");


    //        // 내일 이거 해보깅~!
    //        //PlayerMental mental = GetComponent<PlayerMental>();
    //        //float vertical = Input.GetAxisRaw("Vertical");
    //        //float horizontal = Input.GetAxisRaw("Horizontal");

    //        //if (mental != null && mental.IsReversingControl)
    //        //{
    //        //    vertical = -vertical;
    //        //    horizontal = -horizontal;
    //        //}

    //        //Vector3 moveDirection = forward * vertical + right * horizontal;

    //        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
    //        Vector3 moveDirection = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");

    //        IsMoving = moveDirection.magnitude > 0.1f;

    //        if (IsMoving && !toggleCameraRotation)
    //        {
    //            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
    //            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
    //            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
    //        }

    //        characterController.Move(moveDirection.normalized * (finalSpeed * Time.deltaTime));

    //        float percent = ((run) ? 1f : 0.5f) * moveDirection.magnitude;
    //        animator.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
    //        animator.SetBool("isCrouching", isCrouching);
    //    }
    //}

    private void InputMovement()
    {
        if (isCrouching)
        {
            finalSpeed = crouchSpeed;
            characterController.height = crouchHeight;
            capsuleCollider.height = crouchCapHeight;

            //crouch 상태일 때 center의 Y값을 crouchCenterY로 설정
            characterController.center = new Vector3(originalCenter.x, crouchCenterY, originalCenter.z);
            capsuleCollider.center = new Vector3(originalCapCenter.x, crouchCapCenterY, originalCapCenter.z);

            Timer += Time.deltaTime;
            if (Timer >= 7f)
            {
                isCrouching = false;
                playerCrouch = false;
                Timer = 0f;
            }
        }
        else
        {
            finalSpeed = (run) ? runSpeed : speed;
            characterController.height = originalHeight;
            capsuleCollider.height = originalCapHeight;
            characterController.center = originalCenter;
            capsuleCollider.center = originalCapCenter;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        Vector3 camForward = Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cam.transform.right;

        Vector3 moveDirection = camForward * vertical + camRight * horizontal;

        IsMoving = moveDirection.magnitude > 0.1f;

        if (IsMoving && !toggleCameraRotation)
        {
            float targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        }

        characterController.Move(moveDirection.normalized * (finalSpeed * Time.deltaTime));

        float percent = ((run) ? 1f : 0.5f) * moveDirection.magnitude;
        animator.SetFloat("Blend", percent, 0.1f, Time.deltaTime);
        animator.SetBool("isCrouching", isCrouching);
    }
}
