using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NoteClick : MonoBehaviour
{
    [SerializeField] private int noteIndex = 3;
    [SerializeField] private SouthRoomPuzzleManager manager;

    private void OnMouseDown()
    {
        Debug.Log($"index={noteIndex} clicked");
        manager?.ShowNote(noteIndex);
    }
}
