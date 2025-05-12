using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [Header("requiredKeyTag Settings")]
    //필요 열쇠 태그
    public string requiredKeyTag = "Key";

    //열리는 애니메이터
    //public Animator animator;                

    //private void Awake()
    //{
    //    if (animator == null)
    //        animator = GetComponent<Animator>();
    //}

    //IInteractable 구현
    public void OnInteract(GameObject heldItem)
    {
        //1) 아이템이 널(null)이면 무시
        if (heldItem == null) return;

        //2) 태그가 맞는 열쇠인지 확인
        if (heldItem.CompareTag(requiredKeyTag))
        {
            OpenChest();
        }
        else
        {
            Debug.Log("이건 열쇠가 아닙니다.");
        }
    }

    private void OpenChest()
    {
        Debug.Log("상자가 열립니다!");
        //if (animator != null)
            //animator.SetTrigger("Open");
        //아이템 스폰, 소리 재생 등 추가
    }
}
