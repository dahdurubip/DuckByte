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
        { "clearToBossDoor", new string[] { "어디선가 변화가 생긴 것 같아", } },
        { "clearToJiockdo", new string[] { "닫혀있던 문들이 열린 것 같아", "한번 조사해 보자" } },
        { "interactUnBrokenJar", new string[] { "큰일이다 깨지지않고 소리만 났어", "깨지는 것과 무슨 차이지..?"} },
        { "interactSCDoor", new string[] { "이런.. 잠겨있다", "주변을 조사해보자" } },
        { "interactWell", new string[] { "분명 아까 이곳으로 떨어졌지..?", "무언가 날 덮쳤는데..", "지금은 아무것도 없네.. 뭐였을까..." } },
        { "inPrison", new string[] { "이제 네 차례다","차가운 쇠사슬이 살갗을 파고드는 고통을","숨이 막히는 절망의 순간 순간을 직접 체험해 보아라…" } }
    };
}