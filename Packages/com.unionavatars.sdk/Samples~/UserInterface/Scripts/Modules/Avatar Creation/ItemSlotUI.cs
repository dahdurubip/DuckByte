using System;
using System.Threading;
using System.Threading.Tasks;
using UnionAvatars.API;
using UnityEngine;
using UnityEngine.UI;

namespace UnionAvatars.UI
{
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField]
        protected RawImage rawImage;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;

        public async Task<bool> SetupSlot(Uri imageURL, CancellationTokenSource cancellationToken)
        {
            Texture2D thumbnail = await DownloadItemThumbnail(imageURL, cancellationToken);

            if(thumbnail == null)
                return false;

            rawImage.enabled = true;
            rawImage.texture = thumbnail;
            aspectRatioFitter.aspectRatio = (float)thumbnail.width / (float)thumbnail.height;

            return true;
        }

        private void OnDestroy()
        {
            //Free memory used by Texture Asset
            Destroy(rawImage.texture);
        }

        private async Task<Texture2D> DownloadItemThumbnail(Uri url, CancellationTokenSource cancellationToken)
        {
            try
            {
                byte[] thumbnail = await ResourceDownloader.Download(
                    url,
                    ResourceType.Thumbnail,
                    cancellationToken.Token,
                    timeout: 10
                );

                if (cancellationToken.IsCancellationRequested)
                    return null;

                var itemTex = new Texture2D(2, 2);
                itemTex.wrapMode = TextureWrapMode.Clamp;
                itemTex.LoadImage(thumbnail);

                return itemTex;
            }
            catch
            {
                // TODO: Add fallback image
                // For now we are deleting the slot to prevent issues or confussion
                if(gameObject != null)
                    Destroy(gameObject);

                return null;
            }
        }
    }
}
