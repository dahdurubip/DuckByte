using UnityEngine;

public class EJPlayer : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody 가져오기
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal"); // A(-1) / D(+1)
        float moveZ = Input.GetAxis("Vertical");   // W(+1) / S(-1)

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        rb.linearVelocity = moveDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
    }
}
