using UnityEngine;
using UnityEngine.UI;

public class ObanggiGimmick : MonoBehaviour
{
    // Helper class to group UI elements for each direction
    [System.Serializable] // Makes this class visible and editable in the Inspector when used in an array
    public class DirectionalUIElements
    {
        public string directionName = "Direction"; // For easier identification in Inspector
        public Image slotImage;
        public Button openPopupButton;
        public GameObject popupPanel;
        public Button closePopupButton;
        public Button[] colorButtons; // Assumed to have 4 buttons for 4 colors
    }

    [Header("Main UI")]
    public GameObject mainUIPanel;
    [SerializeField] private Button closeMainUIButton;
    [SerializeField] private GameObject DoorCollider;
    [SerializeField] private GameObject Lock;

    // Instead of individual fields for each direction's UI, use an array.
    // Order in Inspector: 0:East, 1:South, 2:West, 3:North
    [Header("Directional UI Elements (Order: E, S, W, N)")]
    [Tooltip("Populate this array with UI elements for East, South, West, and North in that specific order.")]
    [SerializeField] private DirectionalUIElements[] directionalUIs = new DirectionalUIElements[4];

    [Header("Color Values")]
    [SerializeField] private Color red;
    [SerializeField] private Color blue;
    [SerializeField] private Color white;
    [SerializeField] private Color black;

    private void Start()
    {
        if (DoorCollider != null)
        {
            Collider doorColliderComponent = DoorCollider.GetComponent<Collider>();
            if (doorColliderComponent != null)
            {
                doorColliderComponent.enabled = false;
            }
            else
            {
                Debug.LogError("DoorCollider does not have a Collider component!", DoorCollider);
            }
        }
        else
        {
            Debug.LogError("DoorCollider is not assigned!", this);
        }


        // Main UI Setup
        if (mainUIPanel != null)
        {
            mainUIPanel.SetActive(false);
            if (closeMainUIButton != null)
            {
                closeMainUIButton.onClick.RemoveAllListeners(); // Good practice
                closeMainUIButton.onClick.AddListener(() => mainUIPanel.SetActive(false));
            }
            else
            {
                Debug.LogError("CloseMainUIButton is not assigned!", this);
            }
        }
        else
        {
            Debug.LogError("MainUIPanel is not assigned!", this);
        }


        // Setup for each directional UI
        for (int i = 0; i < directionalUIs.Length; i++)
        {
            DirectionalUIElements currentUI = directionalUIs[i];

            if (currentUI == null)
            {
                Debug.LogError($"DirectionalUI element at index {i} is not assigned!", this);
                continue;
            }

            // Validate essential components for the current UI set
            if (currentUI.popupPanel == null || currentUI.openPopupButton == null ||
                currentUI.closePopupButton == null || currentUI.slotImage == null ||
                currentUI.colorButtons == null || currentUI.colorButtons.Length != 4) // Assuming 4 color buttons
            {
                Debug.LogError($"One or more UI elements for '{currentUI.directionName}' (index {i}) are not assigned or colorButtons count is not 4.", this);
                continue; // Skip this problematic UI set
            }

            currentUI.popupPanel.SetActive(false); // Initialize popups to be closed

            currentUI.openPopupButton.onClick.RemoveAllListeners();
            currentUI.openPopupButton.onClick.AddListener(() => currentUI.popupPanel.SetActive(true));

            currentUI.closePopupButton.onClick.RemoveAllListeners();
            currentUI.closePopupButton.onClick.AddListener(() => currentUI.popupPanel.SetActive(false));

            // Pass the popupPanel to close to SetupColorButtons
            SetupColorButtons(currentUI.colorButtons, currentUI.slotImage, currentUI.popupPanel);

            currentUI.slotImage.color = Color.clear; // Initialize slot images
        }
    }

    private void SetupColorButtons(Button[] buttons, Image targetSlotImage, GameObject popupPanelToClose)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null)
            {
                Debug.LogError($"Color button at index {i} for slot '{targetSlotImage.name}' is not assigned.", targetSlotImage.gameObject);
                continue;
            }

            int buttonIndex = i; // To correctly capture the index in the lambda closure
            buttons[i].onClick.RemoveAllListeners(); // Good practice
            buttons[i].onClick.AddListener(() =>
            {
                Color selectedColor = GetColorByIndex(buttonIndex);
                Debug.Log($"Button '{buttons[buttonIndex].name}' (index {buttonIndex}) clicked for slot '{targetSlotImage.name}' - Selected Color: {selectedColor}");
                targetSlotImage.color = selectedColor;
                if (popupPanelToClose != null)
                {
                    popupPanelToClose.SetActive(false);
                }
                CheckAnswer();
            });
        }
    }

    private Color GetColorByIndex(int index)
    {
        switch (index)
        {
            case 0: return red;
            case 1: return blue;
            case 2: return white;
            case 3: return black;
            default:
                Debug.LogWarning($"Invalid color index: {index}. Returning Color.clear.");
                return Color.clear;
        }
    }

    private void CheckAnswer()
    {
        // Ensure we have exactly 4 directional UI sets for the check
        if (directionalUIs == null || directionalUIs.Length != 4)
        {
            Debug.LogError("CheckAnswer: directionalUIs array is not properly set up (should have 4 elements).", this);
            return;
        }

        // Assuming directionalUIs array order: 0:East, 1:South, 2:West, 3:North
        // And correct colors: East=Blue, South=White, West=Red, North=Black
        if (directionalUIs[0].slotImage.color == blue &&
            directionalUIs[1].slotImage.color == white &&
            directionalUIs[2].slotImage.color == red &&
            directionalUIs[3].slotImage.color == black)
        {
            Debug.Log("정답! 문 열림");
            if (Lock != null)
            {
                Lock.SetActive(false);
            }
            else
            {
                Debug.LogError("Lock GameObject is not assigned!", this);
            }

            if (DoorCollider != null)
            {
                Collider doorColliderComponent = DoorCollider.GetComponent<Collider>();
                if (doorColliderComponent != null)
                {
                    doorColliderComponent.enabled = true;
                }
                // No need for an else here as it's already checked in Start
            }
            // else already handled in Start
        }
        // Optional: Add an else block here if you want to provide feedback for wrong answers.
    }
}
