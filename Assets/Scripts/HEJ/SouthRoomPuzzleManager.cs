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

    [Header("Name Input UI")]
    public GameObject inputCanvas;
    public TMP_InputField nameInputField;
    public Button submitButton;
    public Button cancelButton;

    [Header("Player")]
    public MonoBehaviour playerMovement;

    [Header("Success Item")]
    public GameObject itemPrefab;
    public Transform itemSpawnPoint;

    [Header("Answer")]
    public string correctName;

    private List<string> noteTexts = new List<string>();
    private HashSet<int> readNotes = new HashSet<int>();
    private bool isSolved = false;

    void Start()
    {
        // 노트 내용 코드에서 설정
        noteTexts.Add(@"그녀의 이름을 감히 입 밖으로 내지 말라.
잊혀진 이름은 기억하지 못해야 한다. 
그러나 그 이름이 지워질 때, 
저주는 시작되었다.");
        noteTexts.Add(@"어머니는 깊은 밤마다 먼 곳을 바라보며 누군가를 기다렸다.
바람이 불 때마다 희미한 한숨 속에서 이름의 끝자락만 겨우 흘러나왔다.
끝내 알 수 없던 그 이름의 마지막 음은 '…화'였다.");
        noteTexts.Add(@"그녀가 사라진 후에도 집안의 연못에선 언제나 꽃이 피었다.
그 꽃은 그녀의 그림자처럼 고요히 물 위에 떠 있었고,
사람들은 그 풍경을 보며 어렴풋이 그녀를 떠올리곤 했다.");

        if (noteCanvas != null) noteCanvas.SetActive(false);
        if (inputCanvas != null) inputCanvas.SetActive(false);

        if (closeNoteButton != null) closeNoteButton.onClick.AddListener(CloseNoteUI);
        if (submitButton != null) submitButton.onClick.AddListener(SubmitName);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelInputUI);
    }

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

    public void OpenNameInputUI()
    {
        if (isSolved)
        {
           // Debug.Log("이미 정답을 맞췄습니다. 다시 입력할 수 없습니다.");
            return;
        }

        if (inputCanvas != null) inputCanvas.SetActive(true);
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

        if (!string.IsNullOrEmpty(attempt) &&
            string.Equals(attempt, correctName.Trim(), System.StringComparison.OrdinalIgnoreCase))
        {
            isSolved = true; // 정답 맞춤 기록
            m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
            if (itemPrefab != null && itemSpawnPoint != null)
                Instantiate(itemPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);

            CancelInputUI();

        }
        else
        {
            //Debug.Log("오답입니다.");
        }
    }
}
