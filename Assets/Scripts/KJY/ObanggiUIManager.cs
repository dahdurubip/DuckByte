using UnityEngine;
using UnityEngine.UI;

public class ObanggiUIManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject mainUIPanel;
    [SerializeField] private Button closeMainUIButton;
    [SerializeField] private GameObject DoorCollider;
    [SerializeField] private GameObject Lock;

    [Header("Slot Panels")]
    [SerializeField] private Image slotImage_East;
    [SerializeField] private Button openPopup_East_Button;

    [SerializeField] private Image slotImage_South;
    [SerializeField] private Button openPopup_South_Button;

    [SerializeField] private Image slotImage_West;
    [SerializeField] private Button openPopup_West_Button;

    [SerializeField] private Image slotImage_North;
    [SerializeField] private Button openPopup_North_Button;

    [Header("Popup Panels")]
    [SerializeField] private GameObject popupPanel_East;
    [SerializeField] private Button closePopup_East_Button;
    [SerializeField] private Button[] colorButtons_East;

    [SerializeField] private GameObject popupPanel_South;
    [SerializeField] private Button closePopup_South_Button;
    [SerializeField] private Button[] colorButtons_South;

    [SerializeField] private GameObject popupPanel_West;
    [SerializeField] private Button closePopup_West_Button;
    [SerializeField] private Button[] colorButtons_West;

    [SerializeField] private GameObject popupPanel_North;
    [SerializeField] private Button closePopup_North_Button;
    [SerializeField] private Button[] colorButtons_North;

    [Header("Color Values")]
    [SerializeField] private Color red, blue, white, black;

    private bool _redSouth;
    public bool redSouth
    {
        get { return _redSouth; }
        set
        {
            if (_redSouth != value)
            {
                _redSouth = value;
                UpdateAllColorButtonActivationStates();
            }
        }
    }

    private bool _blueEast;
    public bool blueEast
    {
        get { return _blueEast; }
        set
        {
            if (_blueEast != value)
            {
                _blueEast = value;
                UpdateAllColorButtonActivationStates();
            }
        }
    }

    private bool _whiteWest;
    public bool whiteWest
    {
        get { return _whiteWest; }
        set
        {
            if (_whiteWest != value)
            {
                _whiteWest = value;
                UpdateAllColorButtonActivationStates();
            }
        }
    }

    private bool _blackNorth;
    public bool blackNorth
    {
        get { return _blackNorth; }
        set
        {
            if (_blackNorth != value)
            {
                _blackNorth = value;
                UpdateAllColorButtonActivationStates();
            }
        }
    }

    //모든 방향의 색상 버튼 배열을 담을 배열 (편의용)
    private Button[][] _allDirectionalColorButtons;

    private void Start()
    {
        DoorCollider.GetComponent<Collider>().enabled = false;

        //전체 UI 토글
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

        _allDirectionalColorButtons = new Button[][]
{
            colorButtons_East,
            colorButtons_South,
            colorButtons_West,
            colorButtons_North
};
        UpdateAllColorButtonActivationStates(); //게임 시작 시 버튼 활성화 상태 초기 설정
    }

    private void SetupColorButtons(Button[] buttons, Image targetSlotImage, GameObject popupPanel)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i;
            buttons[i].onClick.RemoveAllListeners(); 
            buttons[i].onClick.AddListener(() =>
            {
                Color selected = GetColorByIndex(idx);
                //Debug.Log($"버튼 {idx} 클릭됨 - 색상: {selected}");
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
            case 2: return white;
            case 3: return black;
            default: return Color.clear;
        }
    }

    private void UpdateAllColorButtonActivationStates()
    {
        if (_allDirectionalColorButtons == null) return;

        foreach (Button[] directionalButtons in _allDirectionalColorButtons)
        {
            if (directionalButtons == null) continue;

            //각 버튼 배열의 길이는 4라고 가정 (red, blue, white, black 순서)
            if (directionalButtons.Length >= 1 && directionalButtons[0] != null) // 빨간색 버튼 (인덱스 0)
            {
                directionalButtons[0].gameObject.SetActive(_redSouth);
            }
            if (directionalButtons.Length >= 2 && directionalButtons[1] != null) // 파란색 버튼 (인덱스 1)
            {
                directionalButtons[1].gameObject.SetActive(_blueEast);
            }
            if (directionalButtons.Length >= 3 && directionalButtons[2] != null) // 흰색 버튼 (인덱스 2)
            {
                directionalButtons[2].gameObject.SetActive(_whiteWest);
            }
            if (directionalButtons.Length >= 4 && directionalButtons[3] != null) // 검은색 버튼 (인덱스 3)
            {
                directionalButtons[3].gameObject.SetActive(_blackNorth);
            }
        }
        //Debug.Log("색상 버튼 활성화 상태 업데이트 완료.");
    }


    private void CheckAnswer()
    {
        if (slotImage_East.color == blue &&
            slotImage_South.color == white &&
            slotImage_West.color == red &&
            slotImage_North.color == black)
        {
            //성공하는 효과음
            //Debug.Log("정답! 문 열림");
            Lock.SetActive(false);
            DoorCollider.GetComponent<Collider>().enabled = true;
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
        }
    }


}
