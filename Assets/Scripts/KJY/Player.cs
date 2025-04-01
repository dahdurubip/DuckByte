using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float finalSpeed;
    [SerializeField] private bool isCrouching = false;
    [SerializeField] public Transform CamTr;
    public bool isMovable = true;

    private Animator animator;
    private CharacterController controller;
    private Vector3 moveDirection;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        InputMovement();
    }

    private void Update()
    {
        HandleCrouch();
        HandleRun();
    }

    private void InputMovement()
    {
        if (!isMovable) return;

        Vector3 forward = CamTr.forward;
        Vector3 right = CamTr.right;

        forward.y = 0;
        right.y = 0;

        float verticalInput = 0f;
        float horizontalInput = 0f;

        if (Input.GetKey(KeyCode.W)) verticalInput += 1f;
        if (Input.GetKey(KeyCode.S)) verticalInput -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput += 1f;
        if (Input.GetKey(KeyCode.A)) horizontalInput -= 1f;

        Vector3 inputDir = (forward * verticalInput + right * horizontalInput).normalized;
        moveDirection = inputDir;

        if (moveDirection.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDirection * finalSpeed * Time.deltaTime);

        // ✅ 애니메이션 한 번만 호출
        SetAnimations( );
    }
    private void SetAnimations()
    {
        if (isCrouching)
        {
            animator.SetBool("CrouchWalk", moveDirection.magnitude > 0);
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            animator.SetBool("BackWalk", false);
        }
        else
        {
            animator.SetBool("CrouchWalk", false);

            if (moveDirection.magnitude > 0)
            {
                // 현재 이동 방향과 카메라 전방 방향의 Dot Product 계산
                float forwardDot = Vector3.Dot(moveDirection, CamTr.forward);
                bool isForward = forwardDot > 0.3f;
                bool isBackward = forwardDot < -0.3f;

                animator.SetBool("Walk", isForward && finalSpeed == moveSpeed);
                animator.SetBool("Run", isForward && finalSpeed == runSpeed);
                animator.SetBool("BackWalk", isBackward);
            }
            else
            {
                animator.SetBool("Walk", false);
                animator.SetBool("Run", false);
                animator.SetBool("BackWalk", false);
            }
        }
    }




    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = !isCrouching;
            animator.SetBool("Crouch", isCrouching);
        }
    }

    private void HandleRun()
    {
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
        {
            finalSpeed = runSpeed;
        }
        else
        {
            finalSpeed = isCrouching ? crouchSpeed : moveSpeed;
        }
    }
}
