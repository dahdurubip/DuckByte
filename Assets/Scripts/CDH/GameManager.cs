using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{

    // 델리게이트 이벤트 (플레이어가 소리를 발생시킬 때 위치를 전달)
    public event Action<Vector3> OnSoundEmitted;
    // C# 기본 제공 델리게이트 함수 - Action<T>

    // public delegate void SoundEvent(Vector3 position);
    // public event SoundEvent OnSoundEmitted;
    // 이걸 한 문장으로 줄인 것임



    // 게임 내에서 소리가 발생하면 호출 (ex: 플레이어 점프, 문 부수기)
    public void EmitSound(Vector3 soundPosition)
    {
        OnSoundEmitted?.Invoke(soundPosition);// OnSoundEmitted가 null이 아니라면 속해있는 함수들을 실행한다.
    }


    // 패닉시 나올 이팩트 혹은 효과음
    public void TriggerPanicEffect()
    {
        //Debug.Log("패닉!");
    }

    public void KillPlayer()
    {
        //Debug.Log("플레이어 죽음");
        SceneLoad.LoadSceneWithLoading("Creature1Map");

    }
}
