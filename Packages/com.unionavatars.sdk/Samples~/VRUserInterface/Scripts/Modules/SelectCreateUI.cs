using UnityEngine;

namespace UnionAvatars.VRUI
{
    public class SelectCreateUI : UIModule
    {
        [SerializeField] private UIModule AvatarSelectionModule;
        [SerializeField] private UIModule AvatarCreationModule;

        public void SelectAvatar()
        {
            SwapModule(AvatarSelectionModule);
        }

        public void CreateAvatar()
        {
            SwapModule(AvatarCreationModule);
        }
    }
}