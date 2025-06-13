using UnityEngine;

public class SkyboxChanger : MonoBehaviour
{
    // 바꿀 Skybox 머티리얼
    public Material targetSkybox;

    // 시그널에 연결할 메서드
    public void ChangeSkybox()
    {
        if (targetSkybox != null)
            RenderSettings.skybox = targetSkybox;
        DynamicGI.UpdateEnvironment();  // GI 갱신
    }
}
