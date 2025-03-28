using TMPro;
using UnionAvatars.API;
using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.VRUI
{
    public class AvatarSlotUI : MonoBehaviour
    {
        [SerializeField]
        private RawImage avatarImage;
        public Button selectButton;
        private bool thumbnail = false;

        private void Awake()
        {
            selectButton = GetComponent<Button>();
        }

        public void SetupAvatarSlot(AvatarMetadata avatar)
        {
            //avatarName.text = avatar.Name;
            //creationDate.text = "Created at: " + avatar.CreatedAt.Date.ToString("dd/MM/yyyy");
            //
            //if(avatar.Lod != null ||avatar.Version < 3)
            //    editButton.interactable = false;
        }

        public void SetAvatarImage(Texture2D image)
        {
            if (image != null)
            {
                avatarImage.texture = image;
                thumbnail = true;
            }
        }

        private void OnDestroy()
        {
            //Free memory used by Texture Asset
            if (thumbnail)
                Destroy(avatarImage.texture);
        }
    }
}
