using UnityEngine;
using System.Collections;


public class Torch : MonoBehaviour, IInteractable
{
    [Header("Wall Movement Settings")]
    [SerializeField] private GameObject FirstWall;
    [SerializeField] private GameObject SecondWall;
    [SerializeField] private float moveDistance = 13f;
    [SerializeField] private float moveSpeed = 3f;

    private Vector3 firstWallTargetPos;
    private Vector3 secondWallTargetPos;
    private bool isActivated = false;
    private bool isMoving = false;

    public void OnInteract(GameObject heldItem)
    {
        if (!isMoving)
            ActivateTorch();
    }


    private void ActivateTorch()
    {
        Vector3 curFirstPos = FirstWall.transform.position;
        Vector3 curSecondPos = SecondWall.transform.position;

        isActivated = !isActivated;
        if (isActivated)
        {
            firstWallTargetPos = curFirstPos + Vector3.up * moveDistance;
            secondWallTargetPos = curSecondPos + Vector3.down * moveDistance;
        }
        else
        {
            firstWallTargetPos = curFirstPos - Vector3.up * moveDistance;
            secondWallTargetPos = curSecondPos - Vector3.down * moveDistance;
        }

        StartCoroutine(MoveWallsSmoothly(curFirstPos, curSecondPos));
    }


    private IEnumerator MoveWallsSmoothly(Vector3 firstStart, Vector3 secondStart)
    {
        isMoving = true;
        float elapsed = 0f;
        float duration = moveDistance / moveSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            //Creature2AudioManager.instance.PlaySfx(Creature2AudioManager.sfx.Pwall);

            FirstWall.transform.position = Vector3.Lerp(firstStart, firstWallTargetPos, t);
            SecondWall.transform.position = Vector3.Lerp(secondStart, secondWallTargetPos, t);

            yield return null;
        }

        //위치 보정
        FirstWall.transform.position = firstWallTargetPos;
        SecondWall.transform.position = secondWallTargetPos;
        isMoving = false;
    }
}
