using System.Collections;
using UnityEngine;

/// <summary>
/// Handles door open/close via Animator or transform movement.
/// Attach this script to your door GameObject.
/// </summary>
public class Door : MonoBehaviour
{
    [Header("Animator Settings (optional)")]
    [SerializeField] private Animator animator;

    [Header("Movement Settings (if no Animator)")]
    [Tooltip("Enable to move door via transform instead of Animator triggers.")]
    [SerializeField] private bool useMovement = false;
    [Tooltip("Offset from closed position when door is open.")]
    [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
    [SerializeField] private float moveSpeed = 2f;

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        // Store the closed and open positions for movement
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    /// <summary>
    /// Toggles the door between open and closed states.
    /// </summary>
    public void Toggle()
    {
        if (animator != null && !useMovement)
        {
            // Use Animator triggers if available
            animator.SetTrigger(isOpen ? "Close" : "Open");
            isOpen = !isOpen;
        }
        else
        {
            // Move transform smoothly
            if (moveCoroutine != null)
                StopCoroutine(moveCoroutine);
            Vector3 target = isOpen ? closedPosition : openPosition;
            moveCoroutine = StartCoroutine(MoveDoor(target));
            isOpen = !isOpen;
        }
    }

    /// <summary>
    /// Explicitly open the door.
    /// </summary>
    public void Open()
    {
        if (!isOpen)
            Toggle();
    }

    /// <summary>
    /// Explicitly close the door.
    /// </summary>
    public void Close()
    {
        if (isOpen)
            Toggle();
    }

    /// <summary>
    /// Returns true if the door is currently open.
    /// </summary>
    public bool IsOpen => isOpen;

    // Smooth movement coroutine
    private IEnumerator MoveDoor(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
    }
}
