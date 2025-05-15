using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoteClick : MonoBehaviour
{
    [Tooltip("쪽지 번호 (0부터)")]
    public int noteIndex;

    [Tooltip("SouthRoomPuzzleManager 참조")]
    public SouthRoomPuzzleManager manager;

    private void OnMouseDown()
    {
        if (manager != null)
            manager.ShowNotePopup(noteIndex);
    }
}