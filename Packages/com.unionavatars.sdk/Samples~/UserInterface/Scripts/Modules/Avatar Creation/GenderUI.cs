using System;
using UnionAvatars.API;

namespace UnionAvatars.UI
{
    public class GenderUI : UIModule
    {
        public UIModule nextModule;

        public void SelectGender(string gender)
        {
            if (Enum.TryParse(gender, out Gender resultGender))
            {
                (parent as CreationBaseUI).Gender = resultGender;
                SwapModule(nextModule);
            }
            else
            {
                throw new ArgumentException($"{gender} is not a valid style");
            }
        }
    }
}
