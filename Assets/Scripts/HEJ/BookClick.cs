using UnityEngine;

public class BookTrigger : MonoBehaviour
{
    [SerializeField] private SouthRoomPuzzleManager puzzleManager;

    private void OnMouseDown()
    {
        // puzzleManager가 할당되어 있으면 이름 입력 UI 열기
        puzzleManager?.OpenInput();
    }
}
