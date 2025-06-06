using UnityEngine;

public class NoteClick : MonoBehaviour
{
    public int noteIndex;
    public SouthRoomPuzzleManager manager;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider != null && hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log("[Note] 클릭 성공! " + noteIndex);
                    if (manager != null)
                        manager.ShowNotePopup(noteIndex);
                }
            }
        }
    }
}
