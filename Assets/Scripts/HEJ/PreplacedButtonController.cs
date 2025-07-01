using UnityEngine;
using UnityEngine.UI;

public class PreplacedButtonController : MonoBehaviour
{
    [Header("씬에 미리 만들어 둔 버튼들 (순서대로)")]
    [SerializeField] private GameObject[] buttonObjects;

    [SerializeField] private ItemManager itemManager;

    [Header("한 버튼에 묶을 아이템 개수")]
    [SerializeField] private int itemsPerButton = 2;

    void Start()
    {
        int totalItems = itemManager.MainItem;
        int buttonCount = Mathf.CeilToInt(totalItems / (float)itemsPerButton);
        int maxButtons = buttonObjects.Length;

        for (int i = 0; i < maxButtons; i++)
        {
            bool shouldActivate;

            if (totalItems <= 2)
            {
                // 2 이하일 때는 가운데(인덱스 1)만 꺼주고 나머지 버튼은 모두 켜기
                shouldActivate = (i != 1);
            }
            else
            {
                // 3개 이상일 때는 itemsPerButton 기준으로 계산된 buttonCount 만큼 순서대로 켜기
                shouldActivate = (i < buttonCount);
            }

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
        Debug.Log($"[{idx + 1}번 버튼] 클릭됨");
    }
}