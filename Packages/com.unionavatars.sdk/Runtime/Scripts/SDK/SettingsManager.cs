using UnityEngine;

namespace UnionAvatars.Settings
{
    public static class SettingsManager
    {
        private static UnionAvatarsSDK_Settings _settings;

        public static UnionAvatarsSDK_Settings Settings
        {
            get { return _settings ?? GetSettings(); }
        }

        private static UnionAvatarsSDK_Settings GetSettings()
        {
            _settings = Resources.Load<UnionAvatarsSDK_Settings>("UnionAvatars/UnionAvatarsSDK_Settings");
            
            return _settings;
        }
    }
}
