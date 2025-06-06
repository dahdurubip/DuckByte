using UnityEngine;

public class NoteClick : MonoBehaviour
{
    public int noteIndex;
    public SouthRoomPuzzleManager manager;

    void OnMouseDown()
    {
        if (manager != null)
        {
            manager.ShowNotePopup(noteIndex);
        }
    }
}
