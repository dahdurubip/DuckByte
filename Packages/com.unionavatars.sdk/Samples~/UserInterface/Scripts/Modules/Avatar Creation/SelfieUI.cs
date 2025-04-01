using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using SFB;
using System.IO;
using UnionAvatars.Utils;
using UnityEngine.Networking;
using System.Collections;
using System.Runtime.InteropServices;
using System.Globalization;

namespace UnionAvatars.UI
{
    public class SelfieUI : UIModule
    {
        [SerializeField]
        private RawImage cameraImage;

        [SerializeField]
        private Button submitButton;

        [SerializeField]
        private AspectRatioFitter aspectRatioFitter;

        [SerializeField]
        private TMP_Dropdown deviceDropdown;

        [SerializeField]
        private GameObject cameraObject;

        [SerializeField]
        private GameObject noDevicesObject;
        private WebCamTexture webcamTexture;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        public static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);
#endif

        private void Start()
        {
            FillWebCamDevices();

            if (deviceDropdown.options.Count <= 0)
                return;

            try
            {
                webcamTexture = new WebCamTexture(
                    deviceDropdown.options[deviceDropdown.value].text
                );
                cameraImage.texture = webcamTexture;
                cameraImage.transform.rotation = Quaternion.AngleAxis(
                    webcamTexture.videoRotationAngle,
                    Vector3.forward
                );
                webcamTexture.Play();
                FixWebcamOrientation();

                aspectRatioFitter.aspectRatio =
                    (float)webcamTexture.width / (float)webcamTexture.height;

                deviceDropdown.onValueChanged.AddListener(ChangeDevice);

                cameraObject.SetActive(true);
                noDevicesObject.SetActive(false);
            }
            catch
            {
                uiManager.session.LogHandler.CustomLog(
                    "Webcam Error",
                    "The selected device is not available"
                );
            }
        }

        private void ChangeDevice(int deviceIndex)
        {
            webcamTexture.Stop();
            webcamTexture.deviceName = deviceDropdown.options[deviceIndex].text;
            webcamTexture.Play();
            FixWebcamOrientation();

            aspectRatioFitter.aspectRatio =
                (float)webcamTexture.width / (float)webcamTexture.height;
        }

        private void FixWebcamOrientation()
        {
            cameraImage.transform.localScale = new Vector3(
                1,
                webcamTexture.videoVerticallyMirrored ? -1 : 1,
                1
            );
            cameraImage.transform.rotation = Quaternion.AngleAxis(
                webcamTexture.videoRotationAngle,
                Vector3.back
            );
        }

        public void TakePicture()
        {
            (Vector2Int portraitOrigin, int portraitHeight) = GetPortraitCoordinates();

            Texture2D snap = new Texture2D(portraitHeight, portraitHeight);

            // Create a temporary RenderTexture.
            RenderTexture tempRenderTexture = RenderTexture.GetTemporary(
                cameraImage.texture.width,
                cameraImage.texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Default
            );
            Graphics.Blit(cameraImage.texture, tempRenderTexture); // Render the original texture to the temporary RenderTexture.

            // Read the pixel data from the temporary RenderTexture.
            RenderTexture.active = tempRenderTexture;
            snap.ReadPixels(
                new Rect(portraitOrigin.x, portraitOrigin.y, portraitHeight, portraitHeight),
                0,
                0
            );

            RotateImage(snap, -webcamTexture.videoRotationAngle);

            snap.Apply();

            (parent as CreationBaseUI).CreateAndSendHeadRequest(
                ImageConverter.ConvertImageToBase64(snap.EncodeToPNG())
            );

            RenderTexture.ReleaseTemporary(tempRenderTexture);
            Destroy(snap);
        }

        private void FillWebCamDevices()
        {
            List<TMP_Dropdown.OptionData> deviceOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var device in WebCamTexture.devices)
            {
                deviceOptions.Add(new TMP_Dropdown.OptionData(device.name));
            }

            deviceDropdown.AddOptions(deviceOptions);
        }

        private (Vector2Int, int) GetPortraitCoordinates()
        {
            //Check shortest side
            if (webcamTexture.width <= webcamTexture.height)
            {
                int yCoordinate = (webcamTexture.height / 2) - (webcamTexture.width / 2);
                return (new Vector2Int(0, yCoordinate), webcamTexture.width);
            }
            else
            {
                int xCoordinate = (webcamTexture.width / 2) - (webcamTexture.height / 2);
                return (new Vector2Int(xCoordinate, 0), webcamTexture.height);
            }
        }

        public void UploadPhoto()
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            var extensions = new[]
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
                new ExtensionFilter("All Files", "*"),
            };
            var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

            if (path == null || path.Length <= 0)
            {
                return;
            }
            else if (File.Exists(path[0]) && IsAnImage(path[0]))
            {
                (parent as CreationBaseUI).CreateAndSendHeadRequest(
                    ImageConverter.ConvertImageToBase64(path[0])
                );
            }
            else
            {
                uiManager.session.LogHandler.CustomLog(
                    "File Error",
                    "The selected file is not a valid image"
                );
            }
#else
            NativeFilePicker.PickFile(
                (path) =>
                {
                    if (File.Exists(path))
                    {
                        (parent as CreationBaseUI).CreateAvatarRequest(
                            ImageConverter.ConvertImageToBase64(path)
                        );
                    }
                },
                new[] { "image/*", "public.image" }
            );
#endif

            // Disable going back to gender selection once creation starts
            previousModule = null;
        }

        public void OnFileUpload(string url)
        {
            StartCoroutine(LoadFileFromURL(url));
        }

        private IEnumerator LoadFileFromURL(string url)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                uiManager.session.LogHandler.CustomLog(
                    "File Error",
                    "The selected file is not a valid image"
                );
            }
            else
            {
                var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                (parent as CreationBaseUI).CreateAndSendHeadRequest(
                    ImageConverter.ConvertImageToBase64(texture.EncodeToPNG())
                );
            }
        }

        public bool IsAnImage(string source)
        {
            return source.EndsWith(".png", true, CultureInfo.CurrentCulture)
                || source.EndsWith(".jpg", true, CultureInfo.CurrentCulture)
                || source.EndsWith(".jpeg", true, CultureInfo.CurrentCulture);
        }

        public static void RotateImage(Texture2D originTexture, float angle)
        {
            int width = originTexture.width;
            int height = originTexture.height;
            float halfHeight = height * 0.5f;
            float halfWidth = width * 0.5f;

            Color32[] originPixels = originTexture.GetPixels32();
            Color32[] rotatedPixels = originTexture.GetPixels32();

            int oldX;
            int oldY;

            float phi = Mathf.Deg2Rad * angle;
            float cosPhi = Mathf.Cos(phi);
            float sinPhi = Mathf.Sin(phi);

            for (int newY = 0; newY < height; newY++)
            {
                for (int newX = 0; newX < width; newX++)
                {
                    rotatedPixels[newY * width + newX] = new Color32(0, 0, 0, 0);
                    int newXNormToCenter = newX - width / 2;
                    int newYNormToCenter = newY - height / 2;
                    oldX = (int)(cosPhi * newXNormToCenter + sinPhi * newYNormToCenter + halfWidth);
                    oldY = (int)(
                        -sinPhi * newXNormToCenter + cosPhi * newYNormToCenter + halfHeight
                    );
                    bool InsideImageBounds =
                        (oldX > -1) && (oldX < width) && (oldY > -1) && (oldY < height);

                    if (InsideImageBounds)
                    {
                        rotatedPixels[newY * width + newX] = originPixels[oldY * width + oldX];
                    }
                }
            }

            originTexture = new Texture2D(width, height);
            originTexture.SetPixels32(rotatedPixels);
            originTexture.Apply();
        }

        protected override void OnExitModule()
        {
            base.OnExitModule();
            if (webcamTexture != null)
                webcamTexture.Stop();
        }
    }
}
