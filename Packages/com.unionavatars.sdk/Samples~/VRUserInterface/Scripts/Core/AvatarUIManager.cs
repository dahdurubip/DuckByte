using System;
using UnionAvatars.API;
using UnityEngine;

namespace UnionAvatars.VRUI
{
    public class AvatarUIManager : MonoBehaviour
    {
        [SerializeField]
        private UIModule baseModule;

        [SerializeField]
        private UIModule startModule;

        [SerializeField]
        private LogManagerUI logManager;
        public ServerSession session;
        public event Action<AvatarMetadata> onAvatarSelected;
        public event Action onClose;

        private UIModule rootModule;

        public void SetupUI(ServerSession session)
        {
            this.session = session;
            session.LogHandler.onLog += logManager.Log;
        }

        public void InstantiateUI()
        {
            //Add the base module to the canvas
            rootModule = Instantiate(baseModule, transform);
            rootModule.transform.SetAsFirstSibling();
            rootModule.EnterModule(null, null, this);

            //Show the first module
            rootModule.SwapChild(startModule);
        }

        public void CloseUI()
        {
            rootModule?.CloseRecursive(rootModule);
            onClose?.Invoke();
        }

        public void ReturnAvatar(AvatarMetadata avatar)
        {
            onAvatarSelected?.Invoke(avatar);
            CloseUI();
        }
    }
}
