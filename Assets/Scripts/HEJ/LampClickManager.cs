using UnityEngine;

public class LampClickManager : MonoBehaviour
{
    public LayerMask lampLayer;
    public EastLampPuzzleManager puzzleManager;
    public float maxDistance = 100f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, lampLayer))
            {
                puzzleManager.TryLamp(hit.collider.gameObject);
            }
        }
    }
}
