using UnityEngine;
using UnityEngine.UI;

public class PreplacedButtonController : MonoBehaviour
{
    [Header("씬에 미리 만들어 둔 버튼들 (순서대로)")]
    [SerializeField] private GameObject[] buttonObjects;

    [SerializeField] private ItemManager itemManager;

    void Start()
    {
        int totalItems = itemManager.MainItem;
        int maxButtons = buttonObjects.Length;
        // 총 켤 버튼 수: totalItems <= 2 ? 2 : 3
        int activateCount = totalItems <= 2 ? 2 : 3;

        for (int i = 0; i < maxButtons; i++)
        {
            bool shouldActivate = (i < activateCount);
            buttonObjects[i].SetActive(shouldActivate);

            if (shouldActivate)
            {
                var btn = buttonObjects[i].GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                int idx = i;
                btn.onClick.AddListener(() => OnButtonClicked(idx));
            }
        }
    }

    private void OnButtonClicked(int idx)
    {
        //Debug.Log($"[{idx + 1}번 버튼] 클릭됨");
    }
}
