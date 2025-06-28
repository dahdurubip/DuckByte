using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SouthRoomPuzzleManager : MonoBehaviour
{
    [Header("Click Popup UI")]
    public GameObject noteCanvas;               // 클릭 시 뜨는 팝업
    public List<RawImage> noteRawImageUIs;      // 클릭용 RawImage 3개

    [Header("Close Button")]
    public Button closeNoteButton;              // 클릭 팝업 닫기

    [Header("Name Input UI")]
    public GameObject inputCanvas;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button cancelButton;

    [Header("Player Movement")]
    public MonoBehaviour playerMovement;

    [Header("Success Item")]
    public GameObject itemPrefab;
    public Transform itemSpawnPoint;

    [Header("Correct Answer")]
    public string correctName;

    private bool isSolved = false;

    void Start()
    {
        // --- 클릭 팝업 초기화 ---
        if (noteCanvas != null) noteCanvas.SetActive(false);
        if (noteRawImageUIs != null)
            foreach (var raw in noteRawImageUIs)
                if (raw != null) raw.gameObject.SetActive(false);
        if (closeNoteButton != null)
            closeNoteButton.onClick.AddListener(CloseNoteUI);

        // --- 이름 입력창 초기화 ---
        if (inputCanvas != null) inputCanvas.SetActive(false);
        if (submitButton != null) submitButton.onClick.AddListener(SubmitName);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelInputUI);
    }

    // 클릭 팝업 열기
    public void ShowNotePopup(int index)
    {
        if (noteCanvas == null || noteRawImageUIs == null) return;
        if (index < 0 || index >= noteRawImageUIs.Count) return;

        for (int i = 0; i < noteRawImageUIs.Count; i++)
        {
            var raw = noteRawImageUIs[i];
            if (raw != null) raw.gameObject.SetActive(i == index);
        }
        noteCanvas.SetActive(true);
    }

    // 클릭 팝업 닫기
    public void CloseNoteUI()
    {
        if (noteCanvas != null) noteCanvas.SetActive(false);
        if (noteRawImageUIs != null)
            foreach (var raw in noteRawImageUIs)
                if (raw != null) raw.gameObject.SetActive(false);
    }

    // 이름 입력창 열기
    public void OpenNameInputUI()
    {
        if (isSolved) return;
        if (inputCanvas != null) inputCanvas.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;
        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
        }
    }

    // 이름 입력 취소
    private void CancelInputUI()
    {
        if (inputCanvas != null) inputCanvas.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
    }

    // 이름 제출
    private void SubmitName()
    {
        if (nameInputField == null) return;
        string attempt = nameInputField.text.Trim();
        if (!string.IsNullOrEmpty(attempt) &&
            System.String.Equals(attempt, correctName.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            isSolved = true;
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
            if (itemPrefab != null && itemSpawnPoint != null)
                Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            CancelInputUI();
        }
        else
        {
            Debug.Log("Incorrect answer: " + attempt);
        }
    }
}
