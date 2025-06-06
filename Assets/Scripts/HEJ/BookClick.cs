using UnityEngine;

public class BookTrigger : MonoBehaviour
{
    public SouthRoomPuzzleManager manager;

    void OnMouseDown()
    {
        if (manager != null)
        {
            manager.OpenNameInputUI();
        }
    }
}
