using System;
using UnityEngine;

namespace UnionAvatars.Log
{
    public class LogHandler
    {
        private bool logToUnity = true;
        public event Action<string, string, AvatarSDKLogType> onLog;

        public LogHandler(bool shouldLogToUnity)
        {
            logToUnity = shouldLogToUnity;
        }

        public void APIWarning(string message)
        {
            CustomLog("API", message, AvatarSDKLogType.Warning);
        }

        public void LoginWarning()
        {
            CustomLog("Login", "You are not logged in", AvatarSDKLogType.Warning);
        }

        public void AvatarWarning(string message)
        {
            CustomLog("Avatar Pipeline", message, AvatarSDKLogType.Error);
        }

        public void UIError(string message)
        {
            CustomLog("User Interface", message, AvatarSDKLogType.Error);
        }

        public void Info(string message)
        {
            CustomLog("Info", message, AvatarSDKLogType.Info);
        }

        public void CustomLog(string title, string message, AvatarSDKLogType type = AvatarSDKLogType.Error)
        {
            onLog?.Invoke(title, message, type);
            if (logToUnity)
                switch (type)
                {
                    case AvatarSDKLogType.Info:
                        Debug.Log($"Union Avatar Log - {title}: {message}");
                        break;
                    case AvatarSDKLogType.Warning:
                        Debug.LogWarning($"Union Avatar Log - {title}: {message}");
                        break;
                    case AvatarSDKLogType.Error:
                        Debug.LogError($"Union Avatar Log - {title}: {message}");
                        break;
                    default:
                        break;
                }
        }
    }

    public enum AvatarSDKLogType
    {
        Info,
        Warning,
        Error,
        Success
    }
}
