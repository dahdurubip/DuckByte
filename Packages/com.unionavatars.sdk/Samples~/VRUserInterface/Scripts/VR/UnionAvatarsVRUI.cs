using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnionAvatars.API;
using UnionAvatars.VRUI;
using UnityEngine.Events;

namespace UnionAvatars.Samples.VR
{
    public class UnionAvatarsVRUI : MonoBehaviour
    {
        [SerializeField] private AvatarUIManager uiManager;
        public string Organization = "YOUR ORGANIZATION ID HERE";
        public UnityEvent<AvatarMetadata> onAvatarSelected;
        public UnityEvent onUIclosed;

        void Start()
        {
            ServerSession session = new ServerSession(Organization, logToUnity: true);
            uiManager.SetupUI(session);
            uiManager.onAvatarSelected += onAvatarSelected.Invoke;
            uiManager.onClose += onUIclosed.Invoke;
        }

        public void ShowUI()
        {
            uiManager.InstantiateUI();
        }
    }
}
