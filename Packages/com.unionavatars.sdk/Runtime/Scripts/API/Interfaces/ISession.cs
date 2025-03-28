using System;
using System.Threading.Tasks;
using UnionAvatars.Log;

namespace UnionAvatars.API
{
    public interface ISession
    {
        public SessionContext SessionContext { get; }
        public LogHandler LogHandler { get; }

        //User
        public Task<bool> Login(string username, string password);
        public Task<bool> Register(string username, string email, string password);
        public Task<User> GetCurrentUser();

        //Assets
        public Task<Brand[]> GetBrands(int? size = null, int page = 1);
        public Task<Catalogue[]> GetCatalogues(int? size = null, int page = 1);
        public Task<Paginated<T>> GetAssets<T>( // TODO: Refactor this
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
            where T : UnionAsset;
        public Task<T> GetAsset<T>(Guid asset)
            where T : UnionAsset;
        public Task<AvatarParts> GetAvatarParts(AvatarMetadata avatar);
        public Task<Outfit> GetOutfit(Guid outfitId);

        //Wardrobe
        public Task<Paginated<T>> GetWardrobe<T>(AssetType type, int size = 20, int page = 1)
            where T : UnionAsset;
        public Task<AssetContainer[]> AddToWardrobe(Guid asset);
        public Task<AssetContainer[]> RemoveFromWardrobe(Guid asset);
        public Task<Outfit> AssembleOutfit(string name, Garment[] garments);
        public Task<Outfit> UpdateOutfit(Guid outfitId, Garment[] garments);

        //Payments
        public Task<CheckoutCreate> CreateCheckout(UnionAsset[] assets);
        public Task<CheckoutStatus> CheckoutStatus(Guid cartId);
        public Task<PaidAssets> GetPaidAssets();

        //Avatars
        public Task<Paginated<AvatarMetadata>> GetAvatars(int size = 20, int page = 1, string sourceType = "");
        public Task<Paginated<AvatarMetadata>> GetDefaultAvatars();
        public Task<AvatarMetadata> GetAvatar(Guid avatarId);
        public Task<AvatarMetadata> CreateAvatar(AvatarRequest avatarRequest);
        public Task<AvatarMetadata> UpdateAvatar(AvatarMetadata avatar);
        public Task DeleteAvatar(Guid avatarId);

        //Heads
        public Task<Head> CreateHead(HeadRequest headRequest);
        public Task<Paginated<Head>> GetHeads(int size = 5, int page = 1);
        public Task<Head> GetHead(Guid headId);
        public Task<Head> UpdateHead(Guid headId, HeadUpdateRequest headUpdateRequest);
        public Task DeleteHead(Guid headId);
    }
}
