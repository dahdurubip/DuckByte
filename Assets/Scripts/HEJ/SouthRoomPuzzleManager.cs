using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SouthRoomPuzzleManager : MonoBehaviour
{
    [Header("Notes")]
    public Canvas noteCanvas;                // note 팝업용
    public TextMeshProUGUI noteTextUI;
    public Button closeNoteButton;
    public List<string> noteTexts;           // 쪽지 내용

    [Header("Name Input UI")]
    public Canvas inputCanvas;               // 이름 입력용 UI (초기 비활성화)
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button cancelButton;              // 새로 추가된 “나가기” 버튼
    public TextMeshProUGUI feedbackText;

    [Header("Player")]
    public PlayerMovement playerMovement;         // 여러분 프로젝트의 이동 스크립트 (예: PlayerMovement)

    [Header("Success Item")]
    public GameObject itemPrefab;
    public Transform itemSpawnPoint;

    [Header("Answer")]
    public string correctName;               // 정답 문자열

    private HashSet<int> readNotes = new HashSet<int>();

    private void Start()
    {
        // UI 초기화
        noteCanvas.gameObject.SetActive(false);
        inputCanvas.gameObject.SetActive(false);
        feedbackText.text = "";

        closeNoteButton.onClick.AddListener(CloseNoteUI);
        submitButton.onClick.AddListener(SubmitName);
        cancelButton.onClick.AddListener(CancelInputUI);
    }

    // NoteClick.cs 에서 호출
    public void ShowNotePopup(int index)
    {
        if (index < 0 || index >= noteTexts.Count) return;

        readNotes.Add(index);
        noteTextUI.text = noteTexts[index];
        noteCanvas.gameObject.SetActive(true);
    }

    private void CloseNoteUI()
    {
        noteCanvas.gameObject.SetActive(false);
    }

    // BookClick.cs 에서 호출
    public void TryOpenNameInput()
    {
        // 언제든 입력 창 열기
        inputCanvas.gameObject.SetActive(true);
        feedbackText.text = "";

        // 이동 스크립트 비활성화
        if (playerMovement != null)
            playerMovement.enabled = false;

        // 입력 필드 포커스
        nameInputField.Select();
        nameInputField.text = "";
    }

    private void CancelInputUI()
    {
        // 입력창 닫고 이동 스크립트 활성화
        inputCanvas.gameObject.SetActive(false);
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    private void SubmitName()
    {
        string attempt = nameInputField.text.Trim();

        if (string.Equals(attempt, correctName, System.StringComparison.Ordinal))
        {
            feedbackText.text = "정답입니다!";
            // 아이템 스폰
            if (itemPrefab != null && itemSpawnPoint != null)
                Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);

            // 입력창 닫고 이동 다시 활성화
            CancelInputUI();
        }
        else
        {
            feedbackText.text = "틀렸습니다. 다시 입력하세요.";
        }
    }
}
