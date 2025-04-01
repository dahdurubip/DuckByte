using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class SetImageBasedOnGender : MonoBehaviour
    {
        [SerializeField]
        private Texture2D maleImage;

        [SerializeField]
        private Texture2D femaleImage;

        [SerializeField]
        private StyleUI styleUI;

        private void Start()
        {
            RawImage rawImage = GetComponent<RawImage>();

            if (styleUI.GetSelectedGender() == API.Gender.male)
            {
                rawImage.texture = maleImage;
            }
            else
            {
                rawImage.texture = femaleImage;
            }
        }
    }
}
