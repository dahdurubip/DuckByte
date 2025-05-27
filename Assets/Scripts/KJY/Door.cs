using UnityEngine;


public class Door : MonoBehaviour
{
    private Animator animator;
    private bool isOpen = false;


    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Toggle()
    {
        //if (animator != null && !useMovement)
        if (animator != null)
        {
            animator.SetTrigger(isOpen ? "Close" : "Open");
            isOpen = !isOpen;
        }
    }

    public void Open()
    {
        if (!isOpen)
        {
            Toggle();
        }
    }

    public void Close()
    {
        if (isOpen)
        {
            Toggle();
        }
    }

}
