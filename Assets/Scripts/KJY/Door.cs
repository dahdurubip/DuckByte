using UnityEngine;

public class Door : MonoBehaviour
{

    //열리는 애니메이터
    private Animator animator;

     
    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void OpenTheLeftDoor( )
    {

        if (animator != null) animator.SetTrigger("Open");
    }
}
