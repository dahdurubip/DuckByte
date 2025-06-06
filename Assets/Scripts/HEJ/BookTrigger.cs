using UnityEngine;

public class BookClick : MonoBehaviour
{
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
                    Debug.Log("[Book] 클릭 성공!");
                    if (manager != null)
                        manager.OpenNameInputUI();
                }
            }
        }
    }
}
