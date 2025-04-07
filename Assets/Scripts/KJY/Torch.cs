using UnityEngine;
using System.Collections;

public class Torch : MonoBehaviour
{
    //올라가는 벽
    [SerializeField] private GameObject FirstWall;
    // 내려가는 벽
    [SerializeField] private GameObject SecondWall; 
    //이동 거리
    [SerializeField] private float moveDistance = 13f;  
    //이동속도
    [SerializeField] private float moveSpeed = 3f;      


    private Vector3 firstWallTargetPos;
    private Vector3 secondWallTargetPos;

    //현재 상태 체크
    private bool isActivated = false; 
    //이동 상태 체크
    private bool isMoving = false; 



    private void OnMouseDown()
    {
        if (!isMoving)
        {
            // 현재 위치를 기준으로 타겟 포지션 설정
            Vector3 currentFirstWallPos = FirstWall.transform.position;
            Vector3 currentSecondWallPos = SecondWall.transform.position;

            // 상태 전환
            isActivated = !isActivated;

            if (isActivated)
            {
                // 올라가기/내려가기
                firstWallTargetPos = currentFirstWallPos + Vector3.up * moveDistance;
                secondWallTargetPos = currentSecondWallPos + Vector3.down * moveDistance;
            }
            else
            {
                // 원래 위치로 돌아가기
                firstWallTargetPos = currentFirstWallPos - Vector3.up * moveDistance;
                secondWallTargetPos = currentSecondWallPos - Vector3.down * moveDistance;
            }

            StartCoroutine(MoveWallsSmoothly(currentFirstWallPos, currentSecondWallPos));
        }
    }

    private IEnumerator MoveWallsSmoothly(Vector3 firstWallInitialPos, Vector3 secondWallInitialPos)
    {
        isMoving = true;
        float elapsedTime = 0f;
        float duration = moveDistance / moveSpeed;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            FirstWall.transform.position = Vector3.Lerp(firstWallInitialPos, firstWallTargetPos, t);
            SecondWall.transform.position = Vector3.Lerp(secondWallInitialPos, secondWallTargetPos, t);
            yield return null;
        }

        // 정확히 위치 보정
        FirstWall.transform.position = firstWallTargetPos;
        SecondWall.transform.position = secondWallTargetPos;

        isMoving = false;
    }

}


