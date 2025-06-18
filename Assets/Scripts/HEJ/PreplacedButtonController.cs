using UnityEngine;
using UnityEngine.UI;

public class PreplacedButtonController : MonoBehaviour
{
    [Header("씬에 미리 만들어 둔 버튼들 (순서대로)")]
    [SerializeField] private GameObject[] buttonObjects;

    [Header("한 버튼에 묶을 아이템 개수")]
    [SerializeField] private int itemsPerButton = 2;

    void Start()
    {

        int totalItems = PlayerPrefs.GetInt("MainItemCount", 0);
        int buttonCount = Mathf.CeilToInt(totalItems / (float)itemsPerButton);

        for (int i = 0; i < buttonObjects.Length; i++)
        {
            bool active = i < buttonCount;
            buttonObjects[i].SetActive(active);

            if (active)
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
