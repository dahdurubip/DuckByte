using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnionAvatars.Log;
using UnionAvatars.Metrics;
using UnionAvatars.Settings;
using UnionAvatars.Utils;

namespace UnionAvatars.API
{
    public class ServerSession : ISession
    {
        //Setup the session
        //This class will contain important information that should be kept alive, such as the session token
        private SessionContext _sessionContext;
        public SessionContext SessionContext
        {
            get => _sessionContext;
        }

        //Setup the logger
        private LogHandler _logHandler;
        public LogHandler LogHandler
        {
            get => _logHandler;
        }

        private CancellationToken cancellationToken;

        /// <summary>
        /// Creates a new Union Avatars session object
        /// </summary>
        /// <param name="url">
        /// URL of the API
        /// </param>
        /// <param name="ct">
        /// Cancellation Token to cancel the ongoing operations on this session
        /// </param>
        /// <param name="logToUnity">
        /// If true, warnings and errors will be logged to Unity
        /// </param>
        public ServerSession(
            string organization,
            string url = "https://api.unionavatars.com/v2/",
            bool logToUnity = true,
            CancellationToken ct = default
        )
        {
            _sessionContext = new SessionContext(url, organization);
            _logHandler = new LogHandler(logToUnity);
            cancellationToken = ct;
        }

        #region User

        /// <summary>
        /// Logs into the union avatars portal
        /// </summary>
        /// <returns>
        /// Bool: successful login
        /// </returns>
        public async Task<bool> Login(string username, string password)
        {
            if (username == "" || password == "")
            {
                LogHandler.CustomLog(
                    "Invalid Credentials",
                    "User and password cannot be empty",
                    AvatarSDKLogType.Error
                );
                return false;
            }

            UnityEngine.WWWForm queryForm = new UnityEngine.WWWForm();
            queryForm.AddField("username", username);
            queryForm.AddField("password", password);
            if (SessionContext.Organization != null)
                queryForm.AddField("organization", SessionContext.Organization);

            WebResponse<AuthToken> loginResponse = await WebRequests.Send<AuthToken>(
                SessionContext.Url + "login",
                "POST",
                queryForm,
                SessionContext,
                cancellationToken
            );

            switch (loginResponse.status)
            {
                case ResponseStatus.Success:
                    SessionContext.Authenticate(loginResponse.data);

                    //Check if user is activated
                    //TODO: CHECK IF ACTIVATED OR NOT!
                    WebResponse userResponse = await WebRequests.Send<string>(
                        SessionContext.Url + "users/me",
                        "GET",
                        SessionContext,
                        cancellationToken
                    );

                    switch (userResponse.status)
                    {
                        case ResponseStatus.Success:
                            //Successful login and verification
                            MetricsLogger.SendMetric(
                                "sdk_login",
                                SessionContext.UserToken,
                                new KeyValuePair<string, string>("user", username)
                            );
                            return true;
                        case ResponseStatus.Failed:
                            LogHandler.APIWarning("User is not active, check your email to verify it");
                            return false;
                        case ResponseStatus.Dropped:
                        default:
                            return false;
                    }
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(loginResponse.responseErrorMessage);
                    return false;
                case ResponseStatus.Dropped:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get the user currently logged in
        /// </summary>
        public async Task<User> GetCurrentUser()
        {
            WebResponse<User> userResponse = await WebRequests.Send<User>(
                SessionContext.Url + "users/me",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (userResponse.status)
            {
                case ResponseStatus.Success:
                    return userResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(userResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Register a new Union Avatars account. It requires an API key in order to work.
        /// </summary>
        /// <returns>
        /// Bool: successful register
        /// </returns>
        public async Task<bool> Register(string username, string email, string password)
        {
            if (SessionContext.ApiKey == null)
            {
                LogHandler.APIWarning(
                    "No API Key provided in the session context. You need a valid API key to register users"
                );
                return false;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>
            {
                { "username", username },
                { "email", email },
                { "password", password },
            };

            WebResponse registerResponse = await WebRequests.Send<string>(
                SessionContext.Url + "users/",
                "POST",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (registerResponse.status)
            {
                case ResponseStatus.Success:
                    return true;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(registerResponse.responseErrorMessage);
                    return false;
                case ResponseStatus.Dropped:
                default:
                    return false;
            }
        }

        #endregion

        #region Assets

        /// <summary>
        /// Returns an array of the active brands
        /// </summary>
        public async Task<Brand[]> GetBrands(int? size = null, int page = 1)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("page", page.ToString())
            };

            if (size != null)
                parameters.Add(new KeyValuePair<string, string>("size", size.ToString()));

            WebResponse<Paginated<Brand>> brandResponse = await WebRequests.Send<Paginated<Brand>>(
                SessionContext.Url + "brands",
                "GET",
                parameters.ToArray(),
                SessionContext,
                cancellationToken
            );

            switch (brandResponse.status)
            {
                case ResponseStatus.Success:
                    return brandResponse.data.Items;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(brandResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns an array of the active catalogues
        /// </summary>
        public async Task<Catalogue[]> GetCatalogues(int? size = null, int page = 1)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("page", page.ToString())
            };

            if (size != null)
                parameters.Add(new KeyValuePair<string, string>("size", size.ToString()));

            WebResponse<Paginated<Catalogue>> catalogueResponse = await WebRequests.Send<Paginated<Catalogue>>(
                SessionContext.Url + "catalogues",
                "GET",
                parameters.ToArray(),
                SessionContext,
                cancellationToken
            );

            switch (catalogueResponse.status)
            {
                case ResponseStatus.Success:
                    return catalogueResponse.data.Items;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(catalogueResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<Paginated<T>> GetAssets<T>(
            Guid catalogue,
            int size = 20,
            int page = 1,
            AssetType type = AssetType.outfits,
            SourceType[] sourceType = null,
            Style style = Style.phr,
            Gender gender = Gender.male,
            int[] version = null,
            Brand brand = null
        )
            where T : UnionAsset
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("size", size.ToString()),
                new KeyValuePair<string, string>("page", page.ToString()),
                new KeyValuePair<string, string>("asset_type", type.ToString()),
                new KeyValuePair<string, string>("style", style.ToString()),
            };

            // If no source_type provided, just show the default assets
            sourceType = sourceType ?? new SourceType[] { SourceType.@default };

            foreach (SourceType value in sourceType)
            {
                parameters.Add(new KeyValuePair<string, string>("source_type", value.ToString()));
            }

            foreach (int value in version)
            {
                parameters.Add(new KeyValuePair<string, string>("version", $"v{value}"));
            }

            if (gender == Gender.male || gender == Gender.female) // Ignore "all"
                parameters.Add(new KeyValuePair<string, string>("gender", gender.ToString()));

            if (brand != null)
                parameters.Add(new KeyValuePair<string, string>("brand_id", brand.Id.ToString()));

            WebResponse<Paginated<AssetContainer>> assetResponse = await WebRequests.Send<Paginated<AssetContainer>>(
                SessionContext.Url + $"catalogues/assets/{catalogue}",
                "GET",
                parameters.ToArray(),
                SessionContext,
                cancellationToken
            );

            switch (assetResponse.status)
            {
                case ResponseStatus.Success:
                    // Cast the AssetContainer array to the desired asset type
                    var filteredTypeItems = assetResponse.data.Items.Where(item => item.AssetType == type);
                    Paginated<T> result = new Paginated<T>()
                    {
                        Page = assetResponse.data.Page,
                        Pages = assetResponse.data.Pages,
                        Size = assetResponse.data.Size,
                        Total = assetResponse.data.Total,
                        Items = filteredTypeItems.Select(item => item.GetAsset<T>()).ToArray()
                    };
                    return result;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(assetResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<T> GetAsset<T>(Guid asset)
            where T : UnionAsset
        {
            WebResponse<AssetContainer> assetResponse = await WebRequests.Send<AssetContainer>(
                SessionContext.Url + $"assets/{asset}",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (assetResponse.status)
            {
                case ResponseStatus.Success:
                    var result = assetResponse.data.GetAsset<T>();
                    if (result == null)
                        LogHandler.CustomLog(
                            "Asset type mismatch",
                            "Specified asset type doesn't match with fetched asset. Returning null",
                            AvatarSDKLogType.Error
                        );
                    return result;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(assetResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<AvatarParts> GetAvatarParts(AvatarMetadata avatar)
        {
            WebResponse<AvatarParts> assetsResponse = await WebRequests.Send<AvatarParts>(
                SessionContext.Url + $"avatars/edit/{avatar.Id}",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (assetsResponse.status)
            {
                case ResponseStatus.Success:
                    return assetsResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(assetsResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<Outfit> GetOutfit(Guid outfitId)
        {
            WebResponse<AssetContainer> outfitResponse = await WebRequests.Send<AssetContainer>(
                SessionContext.Url + $"outfits/{outfitId}",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (outfitResponse.status)
            {
                case ResponseStatus.Success:
                    return outfitResponse.data.Outfit;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(outfitResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }
        #endregion

        #region Wardrobe

        public async Task<Paginated<T>> GetWardrobe<T>(AssetType type, int size = 20, int page = 1)
            where T : UnionAsset
        {
            KeyValuePair<string, string>[] parameters =
            {
                new KeyValuePair<string, string>("size", size.ToString()),
                new KeyValuePair<string, string>("page", page.ToString())
            };

            WebResponse<Paginated<AssetContainer>> wardrobeResponse = await WebRequests.Send<Paginated<AssetContainer>>(
                SessionContext.Url + "wardrobes",
                "GET",
                parameters,
                SessionContext,
                cancellationToken
            );

            switch (wardrobeResponse.status)
            {
                case ResponseStatus.Success:
                    var filteredTypeItems = wardrobeResponse.data.Items.Where(item => item.AssetType == type);
                    Paginated<T> result = new Paginated<T>()
                    {
                        Page = wardrobeResponse.data.Page,
                        Pages = wardrobeResponse.data.Pages,
                        Size = wardrobeResponse.data.Size,
                        Total = wardrobeResponse.data.Total,
                        Items = filteredTypeItems.Select(item => item.GetAsset<T>()).ToArray()
                    };
                    return result;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(wardrobeResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<AssetContainer[]> AddToWardrobe(Guid asset)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "asset_id", asset.ToString() } };

            WebResponse<Paginated<AssetContainer>> wardrobeResponse = await WebRequests.Send<Paginated<AssetContainer>>(
                SessionContext.Url + "wardrobes/",
                "POST",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (wardrobeResponse.status)
            {
                case ResponseStatus.Success:
                    return wardrobeResponse.data.Items;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(wardrobeResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<AssetContainer[]> RemoveFromWardrobe(Guid asset)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "asset_id", asset.ToString() } };

            WebResponse<Paginated<AssetContainer>> wardrobeResponse = await WebRequests.Send<Paginated<AssetContainer>>(
                SessionContext.Url + "wardrobes/",
                "DELETE",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (wardrobeResponse.status)
            {
                case ResponseStatus.Success:
                    return wardrobeResponse.data.Items;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(wardrobeResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<Outfit> AssembleOutfit(string name, Garment[] garments)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string> { { "name", name } };

            if (garments[0] != null)
                parameters.Add("accessories_id", garments[0].Id.ToString());
            if (garments[1] != null)
                parameters.Add("top_id", garments[1].Id.ToString());
            if (garments[2] != null)
                parameters.Add("bottom_id", garments[2].Id.ToString());
            if (garments[3] != null)
                parameters.Add("shoes_id", garments[3].Id.ToString());

            WebResponse<AssetContainer> bodyResponse = await WebRequests.Send<AssetContainer>(
                SessionContext.Url + "outfits/assemble",
                "POST",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (bodyResponse.status)
            {
                case ResponseStatus.Success:
                    return bodyResponse.data.Outfit;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(bodyResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        public async Task<Outfit> UpdateOutfit(Guid outfitId, Garment[] garments)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (garments[0] != null)
                parameters.Add("accessories_id", garments[0].Id.ToString());
            if (garments[1] != null)
                parameters.Add("top_id", garments[1].Id.ToString());
            if (garments[2] != null)
                parameters.Add("bottom_id", garments[2].Id.ToString());
            if (garments[3] != null)
                parameters.Add("shoes_id", garments[3].Id.ToString());

            WebResponse<Outfit> bodyResponse = await WebRequests.Send<Outfit>(
                SessionContext.Url + $"outfits/assemble/{outfitId}",
                "PATCH",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (bodyResponse.status)
            {
                case ResponseStatus.Success:
                    return bodyResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(bodyResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        #endregion

        #region Payments

        /// <summary>
        /// Creates a Stripe checkout object
        /// </summary>
        /// <param name="assets">Array of assets to be paid. Must be of source type 'payable'</param>
        /// <returns></returns>
        public async Task<CheckoutCreate> CreateCheckout(UnionAsset[] assets)
        {
            if (assets.Any((asset) => asset.SourceType != SourceType.payable))
            {
                LogHandler.APIWarning("One or more assets are not of type 'payable'");
                return null;
            }

            Dictionary<string, object> checkoutRequestBody = new Dictionary<string, object>()
            {
                { "products", assets.Select(asset => asset.ContainerId).ToArray() }
            };

            WebResponse<CheckoutCreate> checkoutResponse = await WebRequests.Send<CheckoutCreate>(
                SessionContext.Url + $"payable/checkout?mode=hosted&success_url=https://www.google.com",
                "POST",
                JsonConvert.SerializeObject(checkoutRequestBody),
                SessionContext,
                cancellationToken
            );

            switch (checkoutResponse.status)
            {
                case ResponseStatus.Success:
                    return checkoutResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(checkoutResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Checks if a checkout has been completed (payed)
        /// </summary>
        public async Task<CheckoutStatus> CheckoutStatus(Guid cartId)
        {
            if (cartId == null)
            {
                LogHandler.APIWarning("Empty Cart ID provided");
                return null;
            }

            WebResponse<CheckoutStatus> checkoutResponse = await WebRequests.Send<CheckoutStatus>(
                SessionContext.Url + $"payable/checkout?cart_id={cartId}",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (checkoutResponse.status)
            {
                case ResponseStatus.Success:
                    return checkoutResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(checkoutResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns an array with all the asset IDs the user has purchased
        /// </summary>
        public async Task<PaidAssets> GetPaidAssets()
        {
            WebResponse<PaidAssets> paidAssetsResponse = await WebRequests.Send<PaidAssets>(
                SessionContext.Url + $"payable/assets",
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (paidAssetsResponse.status)
            {
                case ResponseStatus.Success:
                    return paidAssetsResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(paidAssetsResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        #endregion

        #region Avatars

        /// <summary>
        /// Creates a new avatar in the logged account
        /// </summary>
        public async Task<AvatarMetadata> CreateAvatar(AvatarRequest avatarRequest)
        {
            avatarRequest.Optimize = SettingsManager.Settings.enableAvatarOptimization;
            avatarRequest.UseLod = SettingsManager.Settings.enableLOD;
            WebResponse<AvatarMetadata> avatarResponse = await WebRequests.Send<AvatarMetadata>(
                SessionContext.Url + "avatars",
                "POST",
                JsonConvert.SerializeObject(avatarRequest),
                SessionContext,
                cancellationToken
            );

            switch (avatarResponse.status)
            {
                case ResponseStatus.Success:
                    Head avatarHead = await GetHead(avatarResponse.data.HeadId);
                    if (avatarHead != null)
                        avatarResponse.data.Version = avatarHead.Version;
                    return avatarResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(avatarResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Updates an exsiting avatar of the user
        /// </summary>
        /// <param name="avatar">
        /// Updated avatar metadata, it must contain a valid Id and the updated fields
        /// </param>
        /// <returns>
        /// Updated avatar metadata
        /// </returns>
        public async Task<AvatarMetadata> UpdateAvatar(AvatarMetadata avatar)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            if (avatar.Name != null)
                parameters.Add("name", avatar.Name);
            if (avatar.OutfitId != Guid.Empty)
                parameters.Add("outfit_id", avatar.OutfitId.ToString());

            WebResponse<AvatarMetadata> avatarResponse = await WebRequests.Send<AvatarMetadata>(
                SessionContext.Url + "avatars/" + avatar.Id,
                "PATCH",
                JsonConvert.SerializeObject(parameters),
                SessionContext,
                cancellationToken
            );

            switch (avatarResponse.status)
            {
                case ResponseStatus.Success:
                    Head avatarHead = await GetHead(avatarResponse.data.HeadId);
                    if (avatarHead != null)
                        avatarResponse.data.Version = avatarHead.Version;
                    return avatarResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(avatarResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve all the user avatars
        /// </summary>
        /// <param name="limit">
        /// Number of avatars included in the response
        /// </param>
        /// <param name="skip">
        /// Starting index of avatar list
        /// </param>
        public async Task<Paginated<AvatarMetadata>> GetAvatars(int size = 20, int page = 1, string sourceType = "")
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("size", size.ToString()),
                new KeyValuePair<string, string>("page", page.ToString()),
                new KeyValuePair<string, string>("source_type", sourceType)
            };

            // TODO: This needs to be re-written, as currently API doesn't support
            // multiple styles in the query. Now it works because we only have 2 styles
            if (SettingsManager.Settings.enabledStyles == Style.phr)
                parameters.Add(new KeyValuePair<string, string>("style", "phr"));
            else if (SettingsManager.Settings.enabledStyles == Style.crt)
                parameters.Add(new KeyValuePair<string, string>("style", "crt"));

            WebResponse<Paginated<AvatarMetadata>> avatarsResponse = await WebRequests.Send<Paginated<AvatarMetadata>>(
                SessionContext.Url + "avatars",
                "GET",
                parameters.ToArray(),
                SessionContext,
                cancellationToken
            );

            switch (avatarsResponse.status)
            {
                case ResponseStatus.Success:
                    foreach (AvatarMetadata avatar in avatarsResponse.data.Items)
                    {
                        Head avatarHead = await GetHead(avatar.HeadId);
                        if (avatarHead != null)
                            avatar.Version = avatarHead.Version;
                    }
                    return avatarsResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(avatarsResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve all default avatars (for VR)
        /// </summary>
        public async Task<Paginated<AvatarMetadata>> GetDefaultAvatars()
        {
            KeyValuePair<string, string>[] parameters =
            {
                new KeyValuePair<string, string>("size", "100"),
                new KeyValuePair<string, string>("page", "1")
            };

            WebResponse<Paginated<AvatarMetadata>> avatarsResponse = await WebRequests.Send<Paginated<AvatarMetadata>>(
                SessionContext.Url + "avatars/default",
                "GET",
                parameters,
                SessionContext,
                cancellationToken
            );

            switch (avatarsResponse.status)
            {
                case ResponseStatus.Success:
                    return avatarsResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(avatarsResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieves an avatar by it's id
        /// </summary>
        public async Task<AvatarMetadata> GetAvatar(Guid avatarId)
        {
            string endpoint = SessionContext.Url + "avatars/" + avatarId;

            KeyValuePair<string, string>[] parameters = { };
            WebResponse<AvatarMetadata> avatarResponse = await WebRequests.Send<AvatarMetadata>(
                endpoint,
                "GET",
                parameters,
                SessionContext,
                cancellationToken
            );

            switch (avatarResponse.status)
            {
                case ResponseStatus.Success:
                    Head avatarHead = await GetHead(avatarResponse.data.HeadId);
                    if (avatarHead != null)
                        avatarResponse.data.Version = avatarHead.Version;
                    return avatarResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(avatarResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Delete an avatar from the servers
        /// </summary>
        /// /// <param name="avatarId">
        /// The avatar to be deleted
        /// </param>
        public async Task DeleteAvatar(Guid avatarId)
        {
            KeyValuePair<string, string>[] parameters = { };

            WebResponse deleteResponse = await WebRequests.Send<string>(
                SessionContext.Url + "avatars/" + avatarId,
                "DELETE",
                parameters,
                SessionContext,
                cancellationToken
            );

            if (deleteResponse.status == ResponseStatus.Failed)
                LogHandler.APIWarning(deleteResponse.responseErrorMessage);
        }

        #endregion

        #region Heads

        /// <summary>
        /// Creates a new head in the logged account
        /// </summary>
        public async Task<Head> CreateHead(HeadRequest headRequest)
        {
            headRequest.UseLod = SettingsManager.Settings.enableLOD;

            WebResponse<Head> headResponse = await WebRequests.Send<Head>(
                SessionContext.Url + "heads",
                "POST",
                JsonConvert.SerializeObject(headRequest),
                SessionContext,
                cancellationToken
            );

            switch (headResponse.status)
            {
                case ResponseStatus.Success:
                    return headResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(headResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Updates an exsiting avatar of the user
        /// </summary>
        /// <param name="avatar">
        /// Updated avatar metadata, it must contain a valid Id and the updated fields
        /// </param>
        /// <returns>
        /// Updated avatar metadata
        /// </returns>
        public async Task<Head> UpdateHead(Guid headId, HeadUpdateRequest headUpdateRequest)
        {
            WebResponse<Head> headResponse = await WebRequests.Send<Head>(
                SessionContext.Url + "heads/" + headId,
                "PATCH",
                JsonConvert.SerializeObject(headUpdateRequest),
                SessionContext,
                cancellationToken
            );

            switch (headResponse.status)
            {
                case ResponseStatus.Success:
                    return headResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(headResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieve all the bodies metadata
        /// </summary>
        /// <param name="limit">
        /// Number of heads included in the response
        /// </param>
        /// <param name="skip">
        /// Starting index of head list
        /// </param>
        public async Task<Paginated<Head>> GetHeads(int size = 5, int page = 1)
        {
            KeyValuePair<string, string>[] parameters =
            {
                new KeyValuePair<string, string>("size", size.ToString()),
                new KeyValuePair<string, string>("page", page.ToString())
            };
            WebResponse<Paginated<Head>> bodiesResponse = await WebRequests.Send<Paginated<Head>>(
                SessionContext.Url + "heads",
                "GET",
                parameters,
                SessionContext,
                cancellationToken
            );

            switch (bodiesResponse.status)
            {
                case ResponseStatus.Success:
                    return bodiesResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(bodiesResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Retrieves a head by its id
        /// </summary>
        /// <param name="headId">
        /// Guid of the head to be retrieved
        /// </param>
        /// <returns></returns>
        public async Task<Head> GetHead(Guid headId)
        {
            string endpoint = SessionContext.Url + "heads/" + headId.ToString();

            WebResponse<Head> headResponse = await WebRequests.Send<Head>(
                endpoint,
                "GET",
                SessionContext,
                cancellationToken
            );

            switch (headResponse.status)
            {
                case ResponseStatus.Success:
                    return headResponse.data;
                case ResponseStatus.Failed:
                    LogHandler.APIWarning(headResponse.responseErrorMessage);
                    return null;
                case ResponseStatus.Dropped:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Delete a head from the servers
        /// </summary>
        /// /// <param name="headId">
        /// The head to be deleted
        /// </param>
        public async Task DeleteHead(Guid headId)
        {
            KeyValuePair<string, string>[] parameters = { };

            WebResponse deleteResponse = await WebRequests.Send<string>(
                SessionContext.Url + "heads/" + headId,
                "DELETE",
                parameters,
                SessionContext,
                cancellationToken
            );

            if (deleteResponse.status == ResponseStatus.Failed)
                LogHandler.APIWarning(deleteResponse.responseErrorMessage);
        }

        #endregion
    }
}
