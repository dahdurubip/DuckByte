using UnityEngine;

public interface IInteractable
{
    /// <param name="heldItem">플레이어가 현재 손에 든 아이템</param>
    // 아이템 
    void OnInteract(GameObject heldItem);
}

