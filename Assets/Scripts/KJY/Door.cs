using System.Collections;
using UnityEngine;

/// <summary>
/// Handles door open/close via Animator or transform movement.
/// Attach this script to your door GameObject.
/// </summary>
public class Door : MonoBehaviour
{

    //[SerializeField] private bool useMovement = false;
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
    //[SerializeField] private float moveSpeed = 2f;
    private Animator animator;

    private bool isOpen = false;
    //private Vector3 closedPosition;
    //private Vector3 openPosition;
    //private Coroutine moveCoroutine;

    private void Awake()
    {
        //closedPosition = transform.position;
        //openPosition = closedPosition + openOffset;
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
        //else
        //{
        //    if (moveCoroutine != null)
        //        StopCoroutine(moveCoroutine);
        //    Vector3 target = isOpen ? closedPosition : openPosition;
        //    moveCoroutine = StartCoroutine(MoveDoor(target));
        //    isOpen = !isOpen;
        //}
    }


    public void Open()
    {
        if (!isOpen)
            Toggle();
    }


    public void Close()
    {
        if (isOpen)
            Toggle();
    }

    public bool IsOpen => isOpen;


    //private IEnumerator MoveDoor(Vector3 targetPos)
    //{
    //    while (Vector3.Distance(transform.position, targetPos) > 0.01f)
    //    {
    //        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    //        yield return null;
    //    }
    //    transform.position = targetPos;
    //}
}
