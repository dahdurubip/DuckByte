using System;
using UnionAvatars.API;
using UnionAvatars.Settings;
using UnityEngine;

namespace UnionAvatars.UI
{
    public class StyleUI : UIModule
    {
        [SerializeField]
        private GameObject realisticStyleButtons;

        [SerializeField]
        private GameObject cartoonStyleButtons;
        public UIModule nextModule;

        private void Start()
        {
            realisticStyleButtons.SetActive(false);
            cartoonStyleButtons.SetActive(false);

            Style enabledStyles = SettingsManager.Settings.enabledStyles;

            if ((enabledStyles & Style.phr) == Style.phr)
            {
                realisticStyleButtons.SetActive(true);
            }

            if ((enabledStyles & Style.crt) == Style.crt)
            {
                cartoonStyleButtons.SetActive(true);
            }
        }

        public void SelectStyle(string style)
        {
            if (Enum.TryParse(style, out Style resultStyle))
            {
                (parent as CreationBaseUI).Style = resultStyle;
                SwapModule(nextModule);
            }
            else
            {
                throw new ArgumentException($"{style} is not a valid style");
            }
        }

        public Gender GetSelectedGender()
        {
            return (parent as CreationBaseUI).Gender;
        }
    }
}
