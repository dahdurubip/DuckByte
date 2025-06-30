using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SouthRoomPuzzleManager : MonoBehaviour
{
    [SerializeField] private GameObject noteCanvas;
    [SerializeField] private RawImage[] hintNotes;      // 인덱스 0~2: 힌트 노트 그림
    [SerializeField] private TMP_Text fakeNoteText;     // 인덱스 3: 공통 페이크 텍스트
    [SerializeField] private Button closeButton;

    [SerializeField] private GameObject inputCanvas;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button submitButton, cancelButton;

    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemSpawnPoint;

    [SerializeField] private string correctName;

    private bool isSolved = false;

    private void Awake()
    {
        // 버튼 리스너
        closeButton?.onClick.AddListener(CloseNote);
        submitButton?.onClick.AddListener(OnSubmitName);
        cancelButton?.onClick.AddListener(CloseInput);

        // 초기 상태
        noteCanvas.SetActive(false);
        inputCanvas.SetActive(false);
        foreach (var note in hintNotes)
            note.gameObject.SetActive(false);
        fakeNoteText.gameObject.SetActive(false);
    }

    // index 0~2: hintNotes, 3: fakeNoteText
    public void ShowNote(int index)
    {
        ClearNotes();

        switch (index)
        {
            case 0:
            case 1:
            case 2:
                hintNotes[index].gameObject.SetActive(true);
                break;
            case 3:
                fakeNoteText.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Invalid note index: {index}");
                return;
        }

        noteCanvas.SetActive(true);
    }

    private void ClearNotes()
    {
        foreach (var note in hintNotes)
            note.gameObject.SetActive(false);
        fakeNoteText.gameObject.SetActive(false);
    }

    public void CloseNote()
    {
        noteCanvas.SetActive(false);
    }

    public void OpenInput()
    {
        if (isSolved) return;

        inputCanvas.SetActive(true);
        if (playerMovement != null) playerMovement.enabled = false;

        nameInput.text = "";
        nameInput.ActivateInputField();
    }

    private void CloseInput()
    {
        inputCanvas.SetActive(false);
        if (playerMovement != null) playerMovement.enabled = true;
    }

    private void OnSubmitName()
    {
        var attempt = nameInput.text.Trim();
        if (string.Equals(attempt, correctName.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            isSolved = true;
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
            Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
            CloseInput();
        }
        else
        {
            Debug.Log($"Wrong answer: {attempt}");
        }
    }
}
