using UnityEngine;

public class Creature2Manager : MonoBehaviour

{
    [Header("크리처 관련 설정")]
    [SerializeField] private GameObject creature2;
    [SerializeField] private int growThreshold = 3; // 몇 번 감지되면 성장하는지


    private float TheTime = 0;
    private int detectionCount = 0;

    public bool CanTeleportation = false;
    public bool CanGrow = false;

    public void OnPlayerDetected()
    {
        //크리처를 감지된 3초후에 크리처 순간이동
        //크리처가 순간이동하고 나서 횟수 증가
        //만약에 회수 5번 이상이면 크리처 성장한다.
        TheTime += Time.deltaTime;
        Debug.Log($"시간 : {TheTime}");
        Debug.Log($"플레이어 감지됨! 현재 감지 횟수: {detectionCount}");

        //2초되면 횟수 증가하고 
        if(TheTime >= 0.3f)
        {
            //크리처 순간이동한다.
            //횟수 추가한다.
            CanTeleportation = true;
            detectionCount++;
            TheTime = 0;
        }

        if (detectionCount % growThreshold == 0)
        {
            CanGrow = true;
        }
    }




}
