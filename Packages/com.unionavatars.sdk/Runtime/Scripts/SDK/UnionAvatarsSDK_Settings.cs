using UnionAvatars.API;
using UnityEngine;

namespace UnionAvatars.Settings
{
    public class UnionAvatarsSDK_Settings : ScriptableObject
    {
        public bool useCache = false;
        public bool enableAvatarOptimization = false;
        public bool enableLOD = false;

        [Range(0, 3)]
        public int maxLOD = 0;
        public Style enabledStyles = Style.phr | Style.crt;

        [HideInInspector]
        public bool firstTimeLoading = true;

        [HideInInspector]
        public string version = "not specified";

        public void EnableStyle(Style style)
        {
            enabledStyles |= style;
        }

        public void DisableStyle(Style style)
        {
            // Check if the style we want to disable is the only one enabled
            // We need to have at least one style enabled. If so, ignore
            if (enabledStyles == style)
                return;

            enabledStyles &= ~style;
        }
    }
}
