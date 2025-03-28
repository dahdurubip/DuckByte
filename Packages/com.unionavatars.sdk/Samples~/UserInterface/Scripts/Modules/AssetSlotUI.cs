using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnionAvatars.API;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnionAvatars.UI
{
    public class AssetSlotUI : ItemSlotUI, IPointerEnterHandler, IPointerExitHandler
    {
        public TextMeshProUGUI priceTag;
        public CanvasGroup priceTagAlpha;

        public async Task<bool> SetupSlot(
            UnionAsset asset,
            CancellationTokenSource cancellationToken,
            bool owned = false
        )
        {
            if (!await SetupSlot(asset.ThumbnailUrl, cancellationToken))
                return false;

            if (asset.SourceType == SourceType.payable)
            {
                if (owned)
                    priceTag.text = "Owned";
                else
                    priceTag.text = "$" + (asset.Price / 100f)?.ToString("F2");
            }

            return true;
        }

        public void SetPriceTagAlpha(float alpha)
        {
            priceTagAlpha.alpha = alpha;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            SetPriceTagAlpha(1);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            SetPriceTagAlpha(0);
        }
    }
}
