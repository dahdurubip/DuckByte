using UnityEngine;
using System;

public class EJPlayer : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    private int xPressCount = 0;
    private Action escapeCallback;
    private Action eggOneCallback;
    private bool hasEscaped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MovePlayer();

        if (Input.GetKeyDown(KeyCode.X) && !hasEscaped)
        {
            xPressCount++;
            HandleXPress(xPressCount);
        }
    }

    void HandleXPress(int count)
    {
        switch (count)
        {
            case 1:
                OnFirstXPress();
                break;
            case 2:
                OnSecondXPress();
                break;
            case 3:
                OnThirdXPress();
                break;
        }
    }

    void OnFirstXPress()
    {
        if (eggOneCallback != null)
        {
            Debug.Log("EggOne() 호출!");
            eggOneCallback.Invoke();
            eggOneCallback = null; // 한 번만 실행되도록 초기화
        }
    }

    void OnSecondXPress()
    {
        Debug.Log("X 버튼을 두 번째로 눌렀습니다! 한 번만 더 누르면 탈출합니다.");
    }

    void OnThirdXPress()
    {
        Debug.Log("X 버튼 3번 눌림! 탈출 시도...");
        escapeCallback?.Invoke();
        escapeCallback = null;
        hasEscaped = true;
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
        rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
    }

    public void SetEscapeCallback(Action callback)
    {
        escapeCallback = callback;
    }

    public void SetEggOneCallback(Action callback)
    {
        eggOneCallback = callback;
    }

    public void ResetXPressCount()
    {
        xPressCount = 0;
        hasEscaped = false;
    }
}
