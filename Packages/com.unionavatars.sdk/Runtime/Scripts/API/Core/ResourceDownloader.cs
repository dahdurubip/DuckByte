using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnionAvatars.Settings;
using UnityEngine;
using UnityEngine.Networking;

namespace UnionAvatars.API
{
    public class ResourceDownloader
    {
        private static readonly Dictionary<string, SemaphoreSlim> _semaphores = new Dictionary<string, SemaphoreSlim>();

        public static Task<byte[]> Download(
            Uri resourceLink,
            ResourceType resourceType = ResourceType.Unspecified,
            CancellationToken cancellationToken = default,
            Action<byte[]> onCompleted = null,
            string fileId = null,
            DateTimeOffset updateDate = default,
            int timeout = 30
        )
        {
            if (resourceLink == null)
                throw new ArgumentNullException("resourceLink");

            return IsCacheEnabled() && resourceType != ResourceType.Unspecified
                ? DownloadWithCache(
                    resourceLink,
                    resourceType,
                    cancellationToken,
                    onCompleted,
                    fileId,
                    updateDate,
                    timeout
                )
                : DownloadToMemory(resourceLink, cancellationToken, onCompleted, timeout);
        }

        /// <summary>
        /// Downloads or retrieves a resource directly to memory
        /// </summary>
        /// <returns>
        /// Byte[]: Resource data
        /// </returns>
        public static async Task<byte[]> DownloadToMemory(
            Uri resourceLink,
            CancellationToken cancellationToken = default,
            Action<byte[]> onCompleted = null,
            int timeout = 20
        )
        {
            if (resourceLink == null)
                throw new APIOperationFailed("No Resource Link provided");

            using UnityWebRequest resourceWebRequest = UnityWebRequest.Get(resourceLink);

            var byteDownloadHandler = new DownloadHandlerBuffer();

            resourceWebRequest.downloadHandler = byteDownloadHandler;

            resourceWebRequest.timeout = timeout;

            resourceWebRequest.SendWebRequest();

            while (!resourceWebRequest.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;
                await Task.Yield();
            }

            if (resourceWebRequest.result is UnityWebRequest.Result.Success)
            {
                onCompleted?.Invoke(byteDownloadHandler.data);
                return byteDownloadHandler.data;
            }
            else
            {
                throw new APIOperationFailed(resourceWebRequest.error + ", url: " + resourceWebRequest.url);
            }
        }

        /// <summary>
        /// Downloads or retrieves an avatar from cache
        /// </summary>
        /// <returns>
        /// Byte[]: Resource data
        /// </returns>
        private static async Task<byte[]> DownloadWithCache(
            Uri resourceLink,
            ResourceType resourceType,
            CancellationToken cancellationToken = default,
            Action<byte[]> onCompleted = null,
            string fileId = null,
            DateTimeOffset updateDate = default,
            int timeout = 20
        )
        {
            if (resourceLink == null)
                throw new APIOperationFailed("No Resource Link provided");

            //Store the avatar resource file name
            string resourceFileIdentifier = HashString(Path.GetFileNameWithoutExtension(resourceLink.LocalPath));

            string cachePath = "/union_avatars_cache/";
            string fileExtension = ".";

            switch (resourceType)
            {
                case ResourceType.Avatar:
                    cachePath += "avatars/";
                    fileExtension += "glb";
                    break;
                case ResourceType.Body:
                    cachePath += "bodies/";
                    fileExtension += "glb";
                    break;
                case ResourceType.Thumbnail:
                    cachePath += "thumbnails/";
                    fileExtension += "png";
                    break;
                case ResourceType.Garment:
                    cachePath += "garments/";
                    fileExtension += "glb";
                    break;
            }

            CreateDirectory(Application.temporaryCachePath + cachePath);

            var localFilePath =
                Application.temporaryCachePath + cachePath + (fileId ?? resourceFileIdentifier) + fileExtension;

            var semaphore = GetSemaphore(localFilePath);

#if !UNITY_WEBGL
            await semaphore.WaitAsync();
#else
            while (semaphore.CurrentCount <= 0)
            {
                await Task.Yield();
            }
            semaphore.Wait();
#endif

            try
            {
                localFilePath = CompareUpdateTimes(localFilePath, resourceFileIdentifier, updateDate);

                using FileStream fs = new FileStream(
                    localFilePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None
                );

                byte[] bytes = new byte[fs.Length];

                // Check if the avatar exists in cache before downloading it
                if (fs.Length != 0)
                {
#if UNITY_WEBGL && !UNITY_EDITOR || !NET_STANDARD_2_1
                    fs.Read(bytes, 0, (int)fs.Length);
#else
                    await fs.ReadAsync(bytes, 0, (int)fs.Length);
#endif
                    onCompleted?.Invoke(bytes);
                    return bytes;
                }

                using UnityWebRequest resourceWebRequest = UnityWebRequest.Get(resourceLink);
                resourceWebRequest.downloadHandler = new DownloadHandlerBuffer();
                resourceWebRequest.timeout = timeout;

                resourceWebRequest.SendWebRequest();

                while (!resourceWebRequest.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;
                    await Task.Yield();
                }

                if (resourceWebRequest.result is UnityWebRequest.Result.Success)
                {
#if UNITY_WEBGL && !UNITY_EDITOR || !NET_STANDARD_2_1
                    fs.Write(
                        resourceWebRequest.downloadHandler.data,
                        0,
                        (int)resourceWebRequest.downloadHandler.data.Length
                    );
#else
                    await fs.WriteAsync(
                        resourceWebRequest.downloadHandler.data,
                        0,
                        (int)resourceWebRequest.downloadHandler.data.Length
                    );
#endif

                    onCompleted?.Invoke(resourceWebRequest.downloadHandler.data);
                    return resourceWebRequest.downloadHandler.data;
                }
                else
                {
                    throw new APIOperationFailed(resourceWebRequest.error + ", url: " + resourceWebRequest.url);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static string CompareUpdateTimes(string localFilePath, string fileId, DateTimeOffset updateDate)
        {
            string directory = Path.GetDirectoryName(localFilePath);
            string fileName = Path.GetFileNameWithoutExtension(localFilePath);
            string extension = Path.GetExtension(localFilePath);

            string[] files = System
                .IO
                .Directory
                .GetFiles( // Check if fileId is already in cache
                    directory,
                    $"*{fileId}*",
                    System.IO.SearchOption.TopDirectoryOnly
                );

            if (files.Length > 0)
            {
                // Compare dates
                string cacheName = Path.GetFileNameWithoutExtension(files[0]);

                string cacheDateString;

                if (cacheName.Contains("-"))
                    cacheDateString = cacheName.Split('-')[1];
                else
                    cacheDateString = "00020101T000000"; // Minimum possible date

                updateDate = updateDate.AddTicks(-(updateDate.Ticks % TimeSpan.TicksPerSecond)); // Truncate milliseconds

                int dateValue = DateTimeOffset.Compare(
                    updateDate,
                    DateTimeOffset.ParseExact(cacheDateString, "yyyyMMddTHHmmsszz", CultureInfo.InvariantCulture)
                );

                if (dateValue <= 0) // If the cache file is already up to date
                {
                    return files[0];
                }
                else
                {
                    // The file in cache is outdated, so we delete it
                    File.Delete(files[0]);
                }
            }

            // If the cache is outdated or wasn't found, create a new file with the new date

            string newFileName = $"{fileName}-{updateDate.ToString("yyyyMMddTHHmmsszz")}{extension}";
            string newFilePath = Path.Combine(directory, newFileName);

            return newFilePath;
        }

        // Method used as a workaround for avatar update
        public static void ClearAvatarCache(AvatarMetadata avatar)
        {
            var localFilePath = Application.temporaryCachePath + "/cached_avatars/" + avatar.Id + ".glb";
            // Check if the avatar exists in cache before downloading it
            if (File.Exists(localFilePath))
            {
                File.Delete(localFilePath);
            }
        }

        private static SemaphoreSlim GetSemaphore(string path)
        {
            if (_semaphores.ContainsKey(path))
                return _semaphores[path];

            var semaphore = new SemaphoreSlim(1);
            _semaphores[path] = semaphore;
            return semaphore;
        }

        private static void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
                return;
            Directory.CreateDirectory(path);
        }

        private static bool IsCacheEnabled()
        {
            return SettingsManager.Settings != null && SettingsManager.Settings.useCache == true;
        }

        private static string HashString(string text, string salt = "")
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            // Uses SHA256 to create the hash
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

                return hash;
            }
        }
    }

    public enum ResourceType
    {
        Avatar,
        Body,
        Thumbnail,
        Garment,
        Unspecified
    }
}
