using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCAvatar : MonoBehaviour
{
    Animator animator;
    const float minIdleTime = 2;
    const float maxIdleTime = 24;
    const float movementMaxDistance = 10;
    const float movementSpeed = 1;
    const float rotationSpeed = 220;

    private bool isMoving;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(GetNewRoute());
    }

    void Update()
    {
        if (!isMoving)
            return;

        var posStep = movementSpeed * Time.deltaTime;
        var rotStep = rotationSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, posStep);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotStep);

        if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
        }
    }

    private IEnumerator GetNewRoute()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));

            if (isMoving)
                continue;

            isMoving = true;
            animator.SetBool("isWalking", true);
            targetPosition = GetRandomPosition();
            targetRotation = Quaternion.LookRotation(
                targetPosition - transform.position,
                Vector3.up
            );
        }
    }

    private Vector3 GetRandomPosition()
    {
        Vector2 randomPosition = Random.insideUnitCircle * movementMaxDistance;
        return transform.position + new Vector3(randomPosition.x, 0, randomPosition.y);
    }

    private void OnDestroy()
    {
        StopCoroutine(nameof(GetNewRoute));
    }
}
