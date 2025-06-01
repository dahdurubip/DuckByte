//using UnityEngine;
//using UnityEngine.UI;

//public class ObanggiUIManager : MonoBehaviour
//{
//    [Header("전체 UI")]
//    public GameObject mainUIPanel;
//    public Button closeMainUIButton;
//    [SerializeField] private GameObject DoorCollider;
//    [SerializeField] private GameObject Lock;

//    [Header("슬롯")]
//    public Image[] slotImages; // 0: East, 1: South, 2: West, 3: North
//    public Button[] openPopupButtons; // 각 슬롯 옆 버튼

//    [Header("팝업")]
//    public GameObject popupPanel;
//    public Button closePopupButton;
//    public Button[] colorButtons; // 0: Blue, 1: White, 2: Red, 3: Green
//    public Color[] colorValues; // 각 버튼의 실제 색

//    [Header("퍼즐 정답")]
//    public Color eastAnswer ;  // Blue
//    public Color southAnswer; // White
//    public Color westAnswer;  // Red
//    public Color northAnswer; // Green

//    //[Header("정답 판정 시 문 오브젝트")]
//    //public GameObject doorToOpen;

//    private int currentSlotIndex = -1;

//    private void Start()
//    {
//        DoorCollider.GetComponent<Collider>().enabled = false;

//        // 전체 UI 닫기 버튼
//        closeMainUIButton.onClick.AddListener(() => mainUIPanel.SetActive(false));

//        // 각 슬롯 옆 버튼 클릭 -> 팝업 열기
//        for (int i = 0; i < openPopupButtons.Length; i++)
//        {
//            int index = i;
//            openPopupButtons[i].onClick.AddListener(() =>
//            {
//                currentSlotIndex = index;
//                popupPanel.SetActive(true);
//            });
//        }

//        // 팝업 닫기 버튼
//        closePopupButton.onClick.AddListener(() =>
//        {
//            popupPanel.SetActive(false);
//        });

//        // 색상 버튼 클릭
//        for (int i = 0; i < colorButtons.Length; i++)
//        {
//            int colorIndex = i;
//            colorButtons[i].onClick.AddListener(() =>
//            {
//                if (currentSlotIndex >= 0 && currentSlotIndex < slotImages.Length)
//                {
//                    slotImages[currentSlotIndex].color = colorValues[colorIndex];
//                    popupPanel.SetActive(false);
//                    CheckPuzzleAnswer(); // 정답 검사
//                }
//            });
//        }
//    }

//    //private void Update()
//    //{
//    //    if (Input.GetKeyDown(KeyCode.E))
//    //    {
//    //        mainUIPanel.SetActive(true);
//    //    }
//    //}

//    private void CheckPuzzleAnswer()
//    {
//        bool correct =
//            ColorsMatch(slotImages[0].color, eastAnswer) &&
//            ColorsMatch(slotImages[1].color, southAnswer) &&
//            ColorsMatch(slotImages[2].color, westAnswer) &&
//            ColorsMatch(slotImages[3].color, northAnswer);

//        if (correct)
//        {
//            Lock.SetActive(false);
//            DoorCollider.GetComponent<Collider>().enabled = true;
//            //doorToOpen.SetActive(false); // 예시: 문 비활성화 = 열림 처리
//        }
//    }

//    private bool ColorsMatch(Color a, Color b)
//    {
//        return Mathf.Approximately(a.r, b.r) &&
//               Mathf.Approximately(a.g, b.g) &&
//               Mathf.Approximately(a.b, b.b);
//    }
//}


using UnityEngine;
using UnityEngine.UI;

public class ObanggiUIManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject mainUIPanel;
    public Button closeMainUIButton;
    [SerializeField] private GameObject DoorCollider;
    [SerializeField] private GameObject Lock;

    [Header("Slot Panels")]
    public Image slotImage_East;
    public Button openPopup_East_Button;

    public Image slotImage_South;
    public Button openPopup_South_Button;

    public Image slotImage_West;
    public Button openPopup_West_Button;

    public Image slotImage_North;
    public Button openPopup_North_Button;

    [Header("Popup Panels")]
    public GameObject popupPanel_East;
    public Button closePopup_East_Button;
    public Button[] colorButtons_East;

    public GameObject popupPanel_South;
    public Button closePopup_South_Button;
    public Button[] colorButtons_South;

    public GameObject popupPanel_West;
    public Button closePopup_West_Button;
    public Button[] colorButtons_West;

    public GameObject popupPanel_North;
    public Button closePopup_North_Button;
    public Button[] colorButtons_North;

    [Header("Color Values")]
    public Color red, blue, green, white;

    public Sprite baseSprite;

    void Start()
    {
        //slotImage_East.sprite = baseSprite;
        //slotImage_South.sprite = baseSprite;
        //slotImage_West.sprite = baseSprite;
        //slotImage_North.sprite = baseSprite;


        DoorCollider.GetComponent<Collider>().enabled = false;

        // 전체 UI 토글
        mainUIPanel.SetActive(false);
        closeMainUIButton.onClick.AddListener(() => mainUIPanel.SetActive(false));

        openPopup_East_Button.onClick.AddListener(() => popupPanel_East.SetActive(true));
        openPopup_South_Button.onClick.AddListener(() => popupPanel_South.SetActive(true));
        openPopup_West_Button.onClick.AddListener(() => popupPanel_West.SetActive(true));
        openPopup_North_Button.onClick.AddListener(() => popupPanel_North.SetActive(true));

        closePopup_East_Button.onClick.AddListener(() => popupPanel_East.SetActive(false));
        closePopup_South_Button.onClick.AddListener(() => popupPanel_South.SetActive(false));
        closePopup_West_Button.onClick.AddListener(() => popupPanel_West.SetActive(false));
        closePopup_North_Button.onClick.AddListener(() => popupPanel_North.SetActive(false));

        SetupColorButtons(colorButtons_East, slotImage_East, popupPanel_East);
        SetupColorButtons(colorButtons_South, slotImage_South, popupPanel_South);
        SetupColorButtons(colorButtons_West, slotImage_West, popupPanel_West);
        SetupColorButtons(colorButtons_North, slotImage_North, popupPanel_North);

        slotImage_East.color = Color.clear;
        slotImage_South.color = Color.clear;
        slotImage_West.color = Color.clear;
        slotImage_North.color = Color.clear;
    }

    void SetupColorButtons(Button[] buttons, Image targetSlotImage, GameObject popupPanel)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i;
            buttons[i].onClick.RemoveAllListeners(); // ✅ 기존 리스너 제거
            buttons[i].onClick.AddListener(() =>
            {
                Color selected = GetColorByIndex(idx);
                Debug.Log($"버튼 {idx} 클릭됨 - 색상: {selected}");
                targetSlotImage.color = selected;
                popupPanel.SetActive(false);
                CheckAnswer();
            });
        }
    }

    Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 0: return red;
            case 1: return blue;
            case 2: return green;
            case 3: return white;
            default: return Color.clear;
        }
    }

    void CheckAnswer()
    {
        if (slotImage_East.color == blue &&
            slotImage_South.color == white &&
            slotImage_West.color == red &&
            slotImage_North.color == green)
        {
            Debug.Log("정답! 문 열림");
            // GameManager.OpenDoor(); 등으로 문 열기
            Lock.SetActive(false);
            DoorCollider.GetComponent<Collider>().enabled = true;
        }
    }


}
