using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnionAvatars.Utils;

namespace UnionAvatars.API
{
    public partial class User
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("birthday")]
        public object Birthday { get; set; }

        [JsonProperty("country")]
        public object Country { get; set; }

        [JsonProperty("state")]
        public object State { get; set; }

        [JsonProperty("city")]
        public object City { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public abstract class UnionAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        // Since assets are stored in a container, we need to know the container id for some operations
        // This needs improvement for future updates
        public Guid ContainerId { get; set; }

        [JsonProperty("source_type")]
        [JsonConverter(typeof(TolerantStringEnumConverter))]
        public SourceType SourceType { get; set; }

        [JsonProperty("price")]
        public int? Price { get; set; }

        [JsonProperty("style")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Style Style { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("thumbnail_url")]
        public Uri ThumbnailUrl { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        /// <returns>
        /// Integrer of the version number, in case it's invalid it will return -1
        /// </returns>
        public int GetVersionFromName()
        {
            if (!Name.ToLower().StartsWith("v", true, CultureInfo.CurrentCulture))
                return -1;

            return Name[1] - '0';
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AvatarMetadata : UnionAsset
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(Utils.VersionConverter))]
        public int Version = 2;

        [JsonProperty("output_format")]
        [JsonConverter(typeof(TolerantStringEnumConverter))]
        public OutputFormat Output { get; set; } = OutputFormat.GLB;

        [JsonProperty("outfit_id")]
        public Guid OutfitId { get; set; }

        // Since assets are stored in a container, we need to know the container id for some operations
        // This needs improvement for future updates
        //public Guid ContainerId { get; set; }

        [JsonProperty("head_id")]
        public Guid HeadId { get; set; }

        [JsonProperty("url")]
        public Uri AvatarLink { get; set; }

        [JsonProperty("half_body_link")]
        public Uri HalfBodyAvatarLink { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("lod")]
        public Dictionary<string, Uri> Lod { get; set; }

        public AvatarMetadata(Guid id, Guid headId, Gender gender, Style style)
        {
            this.Id = id;
            this.HeadId = headId;
            this.Gender = gender;
            this.Style = style;
        }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AvatarParts
    {
        [JsonProperty("hair")]
        public Hair Hair { get; set; }

        [JsonProperty("outfit")]
        public Outfit Outfit { get; set; }

        [JsonProperty("top")]
        public Garment Top { get; set; }

        [JsonProperty("bottom")]
        public Garment Bottom { get; set; }

        [JsonProperty("shoes")]
        public Garment Shoes { get; set; }

        [JsonProperty("accessories")]
        public Garment Accessories { get; set; }

        [JsonProperty("gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }
    }

    public class Outfit : UnionAsset
    {
        [JsonProperty("obj_metadata")]
        public BodyMetadata Metadata { get; set; }
    }

    public class Garment : UnionAsset
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("place")]
        public string Place { get; set; }

        [JsonProperty("obj_metadata")]
        public BodyMetadata Metadata { get; set; }
    }

    public class BodyMetadata
    {
        [JsonProperty("body")]
        public Dictionary<string, bool> Body { get; set; }
    }

    public class Hair : UnionAsset { }

    public class FacialHair : UnionAsset { }

    public class Attach : UnionAsset
    {
        [JsonProperty("attach_type")]
        public string Type { get; set; }
    }

    public class Head : UnionAsset
    {
        [JsonProperty("version")]
        [JsonConverter(typeof(Utils.VersionConverter))]
        public int Version { get; set; }

        [JsonProperty("output_format")]
        [JsonConverter(typeof(TolerantStringEnumConverter))]
        public OutputFormat Output { get; set; }

        [JsonProperty("hair")]
        public Hair Hair { get; set; }

        [JsonProperty("head_metadata")]
        public HeadMetadata Metadata { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HeadMetadata
    {
        [JsonProperty("gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Gender Gender { get; set; }

        [JsonProperty("hair_color")]
        [JsonConverter(typeof(ColorConverter))]
        public UnityEngine.Color? HairColor { get; set; }
    }

    public class Brand
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("logo")]
        public Uri Logo { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class Catalogue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class AvatarRequest
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("output_format")]
        [JsonConverter(typeof(TolerantStringEnumConverter))]
        public OutputFormat? Output = OutputFormat.GLB;

        [JsonProperty("style")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Style Style = Style.phr;

        [JsonProperty("img")]
        public string Image = null;

        [JsonProperty("head_id")]
        public Guid? HeadId = null;

        [JsonProperty("outfit_id", Required = Required.Always)]
        public Guid OutfitId { get; set; }

        [JsonProperty("create_thumbnail")]
        public bool CreateThumbnail = true;

        [JsonProperty("optimize")]
        public bool? Optimize = null;

        [JsonProperty("use_lod")]
        public bool UseLod = false;
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HeadRequest
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("output_format")]
        [JsonConverter(typeof(TolerantStringEnumConverter))]
        public OutputFormat? HeadOutputFormat = OutputFormat.GLB;

        [JsonProperty("style")]
        [JsonConverter(typeof(StringEnumConverter))]
        public Style Style = Style.phr;

        [JsonProperty("version")]
        [JsonConverter(typeof(Utils.VersionConverter))]
        public int? Version = 3;

        [JsonProperty("selfie_img", Required = Required.Always)]
        public string SelfieImg { get; set; }

        [JsonProperty("hair_id")]
        public Guid? HairId = null;

        [JsonProperty("use_lod")]
        public bool UseLod = false;
    }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class HeadUpdateRequest
    {
        [JsonProperty("hair_id")]
        public Guid? HairId = null;

        [JsonProperty("hair_color")]
        [JsonConverter(typeof(ColorConverter))]
        public UnityEngine.Color? HairColor { get; set; }

        public HeadUpdateRequest (Guid hairId, UnityEngine.Color hairColor)
        {
            HairId = hairId;
            HairColor = hairColor;
        }
    }

    public class Paginated<T>
    {
        [JsonProperty("items")]
        public T[] Items { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("pages")]
        public int Pages { get; set; }
    }

    public class AssetContainer
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("price")]
        public int? Price { get; set; }

        [JsonProperty("garment")]
        public Garment Garment { get; set; }

        [JsonProperty("outfit")]
        public Outfit Outfit { get; set; }

        [JsonProperty("attach")]
        public Attach Attach { get; set; }

        [JsonProperty("hair")]
        public Hair Hair { get; set; }

        [JsonProperty("facial_hair")]
        public FacialHair FacialHair { get; set; }

        [JsonProperty("asset_type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AssetType AssetType;

        public T GetAsset<T>()
            where T : UnionAsset
        {
            switch (AssetType)
            {
                case AssetType.outfits:
                    Outfit.Price = Price;
                    Outfit.ContainerId = Id;
                    return Outfit as T;
                case AssetType.garments:
                    Garment.Price = Price;
                    Garment.ContainerId = Id;
                    return Garment as T;
                case AssetType.hairs:
                    Hair.Price = Price;
                    Hair.ContainerId = Id;
                    return Hair as T;
                case AssetType.attaches:
                    Attach.Price = Price;
                    Attach.ContainerId = Id;
                    return Attach as T;
                case AssetType.facial_hairs:
                    FacialHair.Price = Price;
                    FacialHair.ContainerId = Id;
                    return FacialHair as T;
                default:
                    return null;
            }
        }
    }

    public enum OutputFormat
    {
        GLB,
        FBX
    }

    public enum Gender
    {
        male,
        female,
        all
    }

    [Flags]
    public enum Style
    {
        phr = 1 << 0,
        crt = 1 << 1,
        phr_vr = 1 << 2
    }

    public enum AssetType
    {
        outfits,
        garments,
        hairs,
        attaches,
        facial_hairs
    }

    public enum AvatarBodyPart
    {
        UnionAvatars_Arms_top,
        UnionAvatars_Arms_bottom,
        UnionAvatars_Feet,
        UnionAvatars_Hands,
        UnionAvatars_Legs_bottom,
        UnionAvatars_Legs_top,
        UnionAvatars_Neck,
        UnionAvatars_Hips,
        UnionAvatars_Chest,
        UnionAvatars_Belly
    }

    public enum SourceType
    {
        @default,
        payable,
        custom,
        assembled
    }
}
