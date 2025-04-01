using UnityEngine;

namespace UnionAvatars.UI
{
    public class SetupLayerMask : MonoBehaviour
    {
        public string AvatarLayerName = "Avatar";

        void Start()
        {
            int avatarLayer = LayerMask.NameToLayer(AvatarLayerName);

            var cameraComponent = GetComponent<Camera>();
            var lightComponent = GetComponent<Light>();

            if(cameraComponent != null)
                cameraComponent.cullingMask |= (1 << avatarLayer);
            if(lightComponent != null)
                lightComponent.cullingMask |= (1 << avatarLayer);
        }
    }
}
