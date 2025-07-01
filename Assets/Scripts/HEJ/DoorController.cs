using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform doorMesh;
    private bool isOpen = false;
    private Vector3 initialRotation;


    void Awake()
    {
        if (doorMesh != null)
        {
            initialRotation = doorMesh.localEulerAngles;
            //Debug.Log($"[초기 회전값 저장] doorMesh: {doorMesh.name} → {initialRotation}");
        }
    }

    public void Open()
    {
        Debug.Log($"[DoorController] {gameObject.name} Open() called!");
        isOpen = true;
        if (doorMesh != null)
        {
            doorMesh.localEulerAngles = new Vector3(
                initialRotation.x,
                initialRotation.y + 100f,
                initialRotation.z
            );
           // Debug.Log("[문 열림 처리] 회전 적용됨: " + doorMesh.name);
        }
    }

    public void ResetDoor()
    {
        isOpen = false;
        if (doorMesh != null)
        {
            doorMesh.localEulerAngles = initialRotation;
            //Debug.Log("[문 닫힘 처리] doorMesh 회전 초기화됨: " + doorMesh.name);
        }
        else
        {
           // Debug.LogWarning("[문 닫기 실패] doorMesh가 비어 있음");
        }
    }
}
