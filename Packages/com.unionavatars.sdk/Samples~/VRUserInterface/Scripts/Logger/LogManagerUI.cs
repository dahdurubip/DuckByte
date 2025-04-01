using UnionAvatars.Log;
using UnityEngine;

namespace UnionAvatars.VRUI
{    
    public class LogManagerUI : MonoBehaviour
    {
        [SerializeField] private LogMessageUI LogPrefab;
        [SerializeField] private Transform logLayout;

        public void Log(string title, string message, AvatarSDKLogType logType)
        {
            LogMessageUI newLog = Instantiate(LogPrefab, logLayout);
            newLog.SetupLog(title, message, logType);
            Destroy(newLog.gameObject, 6);
        }
    }
}