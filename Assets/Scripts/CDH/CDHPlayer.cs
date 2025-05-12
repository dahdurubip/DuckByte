using UnityEngine;

public class CDHPlayer : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;
    public bool IsMoving { get; private set; }

    public GameManager GM;
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        IsMoving = input.magnitude > 0.1f;

        controller.Move(input * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.N))
        {
            Jump();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            InteractWithItem();
        }
    }



    void Jump()
    {
        Vector3 soundPosition = transform.position + transform.forward * 2f; // 몬스터가 쫓아올 위치
        GM.EmitSound(soundPosition);  // GameManager에 이벤트 전달
    }

    void InteractWithItem()
    {
        Vector3 soundPosition = transform.position + transform.forward * 2f;
        GM.EmitSound(soundPosition);  // 특정 아이템과 상호작용 시 이벤트 전달
    }
}
