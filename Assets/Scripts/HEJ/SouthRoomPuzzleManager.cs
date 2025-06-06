using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SouthRoomPuzzleManager : MonoBehaviour
{
    [Header("Notes")]
    public GameObject noteCanvas;              
    public TextMeshProUGUI noteTextUI;
    public Button closeNoteButton;
    public List<string> noteTexts;

    [Header("Name Input UI")]
    public GameObject inputCanvas;           
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button cancelButton;
    public TextMeshProUGUI feedbackText;

    [Header("Player")]
    public MonoBehaviour playerMovement;      

    [Header("Success Item")]
    public GameObject itemPrefab;
    public Transform itemSpawnPoint;

    [Header("Answer")]
    public string correctName;

    private HashSet<int> readNotes = new HashSet<int>();

    void Start()
    {
        if (noteCanvas != null) noteCanvas.SetActive(false);
        if (inputCanvas != null) inputCanvas.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";

        if (closeNoteButton != null) closeNoteButton.onClick.AddListener(CloseNoteUI);
        if (submitButton != null) submitButton.onClick.AddListener(SubmitName);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelInputUI);
    }

    // 쪽지 클릭에서 호출
    public void ShowNotePopup(int index)
    {
        if (index < 0 || index >= noteTexts.Count) return;
        readNotes.Add(index);
        if (noteTextUI != null) noteTextUI.text = noteTexts[index];
        if (noteCanvas != null) noteCanvas.SetActive(true);
    }

    private void CloseNoteUI()
    {
        if (noteCanvas != null) noteCanvas.SetActive(false);
    }

    // 책 클릭에서 호출
    public void OpenNameInputUI()
    {
        if (inputCanvas != null) inputCanvas.SetActive(true);
        if (feedbackText != null) feedbackText.text = "";
        if (playerMovement != null) playerMovement.enabled = false;
        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
        }
    }

    private void CancelInputUI()
    {
        if (inputCanvas != null) inputCanvas.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
    }

    private void SubmitName()
    {
        string attempt = nameInputField != null ? nameInputField.text.Trim() : "";

        if (string.Equals(attempt, correctName, System.StringComparison.Ordinal))
        {
            if (feedbackText != null) feedbackText.text = "정답입니다!";
            if (itemPrefab != null && itemSpawnPoint != null)
                Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            CancelInputUI();
        }
        else
        {
            if (feedbackText != null) feedbackText.text = "틀렸습니다. 다시 입력하세요.";
        }
    }
}
