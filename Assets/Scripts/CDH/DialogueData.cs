using System.Collections.Generic;
using UnityEngine;

public class DialogueData : MonoBehaviour
{
    // 각 페이즈별 대사를 Dictionary로 관리
    public Dictionary<int, string[]> phaseDialogues = new Dictionary<int, string[]>()
    {
        { 2, new string[] { "이럴 수가...", "내가 밀리다니!" } },
        { 3, new string[] { "크윽... 이 힘은?", "너 정말 인간이냐?" } },
        { 4, new string[] { "으아아악!", "이건... 끝이 아니다..." } }
    };

    // 다른 용도의 대사들도 여기서 관리 가능
}
