using UnityEngine;

public class Creature2Manager : MonoBehaviour

{

    [SerializeField] private WallPosController wallpos;
    public bool wallmove = false;

    [Header("크리처 관련 설정")]
    [SerializeField] private GameObject creature2;
    [SerializeField] private int growThreshold = 3; // 몇 번 감지되면 성장하는지


    private float TheTime = 0;
    public int detectionCount = 0;

    public bool CanTeleportation = false;
    public bool CanGrow = false;

    public bool TheLight = false;

    private void Update()
    {
        if(wallmove)
        {
            wallpos.MoveThewall(detectionCount);
            wallmove = false;
        }
    }

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
            detectionCount++;
            MoveAndGrow();
            TheTime = 0;
        }

    }

    private void MoveAndGrow()
    {
        float Timer = 0;
        if(detectionCount == 1)
        {
            Timer += Time.deltaTime;
            if(Timer >= 2f)
            {
                CanTeleportation = true;
            }
        }
        if (detectionCount % growThreshold == 0)
        {
            CanGrow = true;
        }

    }

    //플레이어스크립트에서 호출해야 함
    public void WhenTheLightOn()
    {
        float TheTimer = 0f;
        if(TheLight)
        {
            TheTimer += Time.deltaTime;
            if(TheTimer >= 3f)
            {
                //손전등 3초 이상 켰다. 크리처 순간이동 해야 함.
                CanTeleportation = true;
            }
        }
    }


}
