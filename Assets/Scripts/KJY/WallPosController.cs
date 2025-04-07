using UnityEngine;

public class WallPosController : MonoBehaviour
{

    [SerializeField] private GameObject[] wall;
    [SerializeField] private GameObject[] fixedwall;

    [SerializeField] private float moveDistance = 13f;

    private Vector3 tarfetPos;


    public void MoveThewall(int Cnt)
    {
        if(Cnt == 3)
        {
            wallPos();
        }
        else if (Cnt == 6)
        {
            otherWallPos();
        }
        else if (Cnt == 9)
        {
            fixedWallPos();
        }
    }

    private void wallPos( )
    {
        for (int i = 0; i < 2; ++i )
        {
            Move(i);
        }
    }

    private void otherWallPos()
    {
        for (int i = 2; i < wall.Length; ++i)
        {
            Move(i);
        }
    }

    private void fixedWallPos()
    {
        for(int i = 0; i < fixedwall.Length; ++i)
        {
            Move(i);
        }
    }

    private void Move(int a)
    {
        tarfetPos = wall[a].transform.position;
        tarfetPos.y = moveDistance;
        wall[a].transform.position = tarfetPos;
    }
}
