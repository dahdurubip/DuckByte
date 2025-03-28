//using UnityEngine;

//public class Creature2Manager : MonoBehaviour
//{
//    //눈알 감지 횟수 
//    private int totalDetectionCount = 0; 

//    public void IncreaseDetectionCount()
//    {
//        //감지 횟수 증가
//        totalDetectionCount++; 
//        Debug.Log($"총 감지 횟수: {totalDetectionCount}");

//        //감지 횟수에 따라 크리처 성장
//        UpdateCreatureState();
//    }

//    private void UpdateCreatureState()
//    {
//        if (totalDetectionCount > 5)
//        {
//            Debug.Log("크리처가 강해진다");
//            //크리처 성장 관련 로직 추가 (예: 속도 증가, 공격력 증가 등)
//        }
//    }

//    //감지 횟수 가져오기
//    public int GetDetectionCount() => totalDetectionCount;
//}
using UnityEngine;

public class Creature2Manager : MonoBehaviour
{
    internal static object instance;
    [SerializeField] private int detectionThreshold = 5; // 예시: 5번 감지되면 성장
    private int totalDetectionCount = 0;

    // 크리처의 단계나 상태를 표현할 수 있음
    private int creatureStage = 1;

    public void IncreaseDetectionCount()
    {
        totalDetectionCount++;
        Debug.Log($"총 감지 횟수: {totalDetectionCount}");

        CheckGrowth();
    }

    private void CheckGrowth()
    {
        if (totalDetectionCount >= detectionThreshold)
        {
            GrowCreature();
            totalDetectionCount = 0; // 초기화하거나 누적 유지 선택 가능
        }
    }

    private void GrowCreature()
    {
        creatureStage++;
        Debug.Log($"크리처가 성장! 현재 단계: {creatureStage}");

        // 예시로 크리처 속도 증가 또는 외형 변경 가능
        // 예: this.GetComponent<CreatureBehavior>().IncreaseSpeed();
    }
}
