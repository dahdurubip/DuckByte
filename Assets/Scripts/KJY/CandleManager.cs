using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class CandleManager : MonoBehaviour
{
    [Header("candleList")]
    public List<Candle> candles = new List<Candle>();

    [Header("correctAnswer")]
    public List<int> correctIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

    [Header("particle")]
    [SerializeField] private ParticleSystem particle;

    [Header("northRoomItem")]
    [SerializeField] private GameObject blackNorth;

    [Header("doorCollider")]
    [SerializeField] private Collider northRoomDoor;

    [Header("volume")]
    [SerializeField] private GameObject volumeBlack;

    [SerializeField] private ItemManager itemManager;

    //현재까지 켜진 촛불 인덱스를 순서대로 저장
    private List<int> litIndices = new List<int>();


    private void Start()
    {
        particle.Stop();
        northRoomDoor.enabled = false;
        for (int i = 0; i < candles.Count; i++)
        {
            candles[i].Initialize(this, i);
        }
    }

    //Candle이 켜졌을 때 호출됩니다.
    public void OnCandleLit(int index)
    {
        //중복 방지: 이미 기록된 인덱스는 무시
        if (litIndices.Contains(index)) return;

        //새로 켜진 촛불 인덱스 추가
        litIndices.Add(index);

        //정답 개수와 일치할 때만 판정
        if (litIndices.Count == correctIndices.Count)
        {
            //순서와 값이 모두 일치하면 성공, 아니면 오답
            if (litIndices.SequenceEqual(correctIndices))
                Correct();
            else
                Wrong();
        }
    }

    private void Correct()
    {
        //Debug.Log("정답! 퍼즐이 풀렸습니다.");
        itemManager.DestroyCurrentItem();
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.northRbigFire);
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.clearSound);
        blackNorth.SetActive(true);
        volumeBlack.SetActive(false);
        particle.Play();
        northRoomDoor.enabled = true;
    }

    private void Wrong()
    {
        Debug.Log("다시 시도 필요");
        m1_AudioManager.instance.PlaySfx(m1_AudioManager.m1sfx.northRwomanCry);
        //모든 촛불 초기화
        foreach (var candle in candles)
            candle.ResetCandle();

        //입력 기록 초기화
        litIndices.Clear();
    }
}
