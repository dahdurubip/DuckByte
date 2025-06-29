using UnityEngine;

public class NoteClick : MonoBehaviour
{
    public int noteIndex;
    public SouthRoomPuzzleManager manager;

    void OnMouseDown()
    {
        manager?.ShowNotePopup(noteIndex);
    }
}
