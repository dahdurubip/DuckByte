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
    public Dictionary<string, string[]> interactables = new Dictionary<string, string[]>()
    {
        { "goToBossDoor", new string[] { "뭐지...", "어떤 힘에 의해 막혀있는 것 같아", "들어갈 방법을 찾아보자"} },
        { "interactUnBrokenJar", new string[] { "큰일이다 깨지지않고 소리만 났어", "깨지는 것과 무슨 차이지..?"} }
    };
}
