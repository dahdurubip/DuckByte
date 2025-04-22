using UnityEngine;

public class LanternInteract : MonoBehaviour
{
    public int lanternIndex; // 이 석등의 순번
    public BlueRoom puzzleManager;

    private void OnMouseDown()
    {
        puzzleManager.SelectLantern(lanternIndex);
    }
}
