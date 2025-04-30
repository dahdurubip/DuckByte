using UnityEngine;
using System.Collections.Generic;

public class CandleManager : MonoBehaviour
{
    [Header("모든 Candle을 순서대로 할당하세요 (Hierarchy 순서대로)")]
    public List<Candle> candles = new List<Candle>();

    [Header("정답으로 켜야 할 인덱스 (0부터 시작)")]
    public List<int> correctIndices = new List<int> { 0, 2, 4, 5, 8 };

    // 현재까지 켜진 촛불 인덱스
    private HashSet<int> litIndices = new HashSet<int>();

    void Start()
    {
        // 각 Candle에 이 매니저를 등록
        for (int i = 0; i < candles.Count; i++)
        {
            candles[i].Initialize(this, i);
        }
    }

    /// <summary>
    /// Candle이 켜졌을 때 호출됩니다.
    /// </summary>
    public void OnCandleLit(int index)
    {
        // 이미 판정이 끝난 촛불은 무시
        if (litIndices.Contains(index)) return;

        // 올바른 인덱스면 추가, 아니면 바로 오답 처리
        if (correctIndices.Contains(index))
        {
            litIndices.Add(index);
            CheckComplete();
        }
        else
        {
            Wrong();
        }
    }

    private void CheckComplete()
    {
        // 모두 켜졌다면 성공
        if (litIndices.Count == correctIndices.Count)
            Correct();
    }

    private void Correct()
    {
        Debug.Log("정답! 퍼즐이 풀렸습니다.");
        // TODO: 성공 이펙트, 다음 단계로 이동 등
    }

    private void Wrong()
    {
        Debug.Log("오답! 모든 촛불을 끄고 다시 시도하세요.");
        // 모든 촛불 끄기
        foreach (var c in candles)
            c.ResetCandle();

        // 판정 초기화
        litIndices.Clear();
    }
}
