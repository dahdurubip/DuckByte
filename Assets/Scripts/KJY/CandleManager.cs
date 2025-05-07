using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CandleManager : MonoBehaviour
{
    [Header("모든 Candle을 순서대로 할당하세요 (Hierarchy 순서대로)")]
    public List<Candle> candles = new List<Candle>();

    [Header("정답으로 켜야 할 인덱스 (0부터 시작, 순서 중요)")]
    public List<int> correctIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7};

    // 현재까지 켜진 촛불 인덱스를 순서대로 기록
    private List<int> litIndices = new List<int>();

    public ParticleSystem particle;


    private void Awake()
    {
        //particle = GetComponent<ParticleSystem>();
    }


    void Start()
    {
        particle.Stop();
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
        // 중복 방지: 이미 기록된 인덱스는 무시
        if (litIndices.Contains(index)) return;

        // 새로 켜진 촛불 인덱스 추가
        litIndices.Add(index);

        // 정답 개수와 일치할 때만 판정
        if (litIndices.Count == correctIndices.Count)
        {
            // 순서와 값이 모두 일치하면 성공, 아니면 오답
            if (litIndices.SequenceEqual(correctIndices))
                Correct();
            else
                Wrong();
        }
    }

    private void Correct()
    {
        Debug.Log("정답! 퍼즐이 풀렸습니다.");
        // TODO: 성공 이펙트, 다음 단계로 이동 등
        particle.Play();
    }

    private void Wrong()
    {
        Debug.Log("오답! 모든 촛불을 끄고 다시 시도하세요.");
        // 모든 촛불 초기화
        foreach (var c in candles)
            c.ResetCandle();

        // 입력 기록 초기화
        litIndices.Clear();
    }
}
