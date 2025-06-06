using UnityEngine;

public class SCDoor : MonoBehaviour, IInteractable
{
    [Header("requiredKeyTag Settings")]
    //필요 열쇠 태그
    public string requiredKeyTag = "SCDoorKey";
    [SerializeField] private ItemManager itemmanager;

    [SerializeField] private GameObject SCDoorLock;

    public GameObject door1;
    public GameObject door2;

    //IInteractable 구현
    public void OnInteract(GameObject heldItem)
    {
        //1) 아이템이 널(null)이면 무시
        if (heldItem == null) return;

        //2) 태그가 맞는 열쇠인지 확인
        if (heldItem.CompareTag(requiredKeyTag))
        {
            OpenSCD();
        }
        else
        {
            Debug.Log("이건 열쇠가 아닙니다.");
        }
    }

    private void OpenSCD()
    {
        Debug.Log("자물쇠가 풀렸습니다");
        Destroy(itemmanager.currentItem);
        // 풀린 자물쇠가 사라짐
        SCDoorLock.SetActive(false);
        // 문과 상호작용 할 수 있게됨
        door1.layer = LayerMask.NameToLayer("Interactable");
        door2.layer = LayerMask.NameToLayer("Interactable");
    }
}
