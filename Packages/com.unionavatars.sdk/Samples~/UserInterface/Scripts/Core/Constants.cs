using System;
using System.Collections.Generic;
using UnionAvatars.API;
using UnityEngine;

namespace UnionAvatars.UI
{
    public class Constants
    {
        private const string bodyPartsBaseURL =
            "https://union-web-asset.s3.eu-central-1.amazonaws.com/avatar_inventory/{0}/{1}/v{2}/{3}.glb";

        private static Dictionary<string, Dictionary<Gender, Dictionary<Style, string>>> defaultAssetIds =
            new Dictionary<string, Dictionary<Gender, Dictionary<Style, string>>>()
            {
                {
                    "Outfit",
                    new Dictionary<Gender, Dictionary<Style, string>>()
                    {
                        {
                            Gender.male,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "18b2f2d3-f95a-4475-9d88-4c942705143a" },
                                { Style.crt, "64ea198a-9958-4466-8f1e-e75379f49ad0" }
                            }
                        },
                        {
                            Gender.female,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "d1441296-1a0b-4140-a8f1-a4e33854c6fa" },
                                { Style.crt, "86d6972b-24b3-48a0-866a-bfe30e5b07f4" }
                            }
                        }
                    }
                },
                {
                    "Top",
                    new Dictionary<Gender, Dictionary<Style, string>>()
                    {
                        {
                            Gender.male,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "451fcc01-2982-43c1-8b7d-f4b1322bb8ed" },
                                { Style.crt, "13ef14d1-a095-4580-abc6-1262e4854ed1" }
                            }
                        },
                        {
                            Gender.female,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "6ad5c8e1-a507-4f2e-8eb2-a0af35d6fd9f" },
                                { Style.crt, "bef4ae07-7336-49d2-bb0f-e5570a4643d7" }
                            }
                        }
                    }
                },
                {
                    "Bottom",
                    new Dictionary<Gender, Dictionary<Style, string>>()
                    {
                        {
                            Gender.male,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "58372f81-c3b8-4a5d-b54c-6e60dab4f980" },
                                { Style.crt, "5cd5822c-1fd0-4cf7-aa03-08f854103899" }
                            }
                        },
                        {
                            Gender.female,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "9bc7e37d-7878-4486-9fab-cbcc712a09eb" },
                                { Style.crt, "4765bcbb-3169-4de4-8905-0d9276e6b82e" }
                            }
                        }
                    }
                },
                {
                    "Shoes",
                    new Dictionary<Gender, Dictionary<Style, string>>()
                    {
                        {
                            Gender.male,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "7285bd77-780c-4343-b122-e8c26b7ae7d4" },
                                { Style.crt, "7a7a6bd0-2509-4219-9300-c01de928bb91" }
                            }
                        },
                        {
                            Gender.female,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "407f4d26-3ce5-420f-abd0-489ad85e2902" },
                                { Style.crt, "34b253e4-4e65-4caf-ac29-9a021d48df9c" }
                            }
                        }
                    }
                },
                {
                    "Hair",
                    new Dictionary<Gender, Dictionary<Style, string>>()
                    {
                        {
                            Gender.male,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "a5e7ec71-11b9-40ab-94f8-89421428976a" },
                                { Style.crt, "dd60fff9-7c8b-4cb9-91f7-bbe6fe65df07" }
                            }
                        },
                        {
                            Gender.female,
                            new Dictionary<Style, string>()
                            {
                                { Style.phr, "29e4eb21-023e-4781-bfda-8f0ce7b687d4" },
                                { Style.crt, "6b004816-7bc2-4940-8d20-cabe8713b51b" }
                            }
                        }
                    }
                },
            };

        public static Dictionary<Style, Vector3> headAssemblyPosition =
            new Dictionary<Style, Vector3>()
            {
                { Style.phr, new Vector3(0, 1.70537f, -0.039149f) },
                { Style.crt, new Vector3(0, 1.675f, -0.08f) }
            };

        public static Dictionary<Style, Vector3> headAssemblyScale =
            new Dictionary<Style, Vector3>() { { Style.phr, new Vector3(1.02266f, 1.02266f, 1.02266f) }, { Style.crt, Vector3.one } };

        public static string GetAvatarBodyPartURL(Gender gender, Style style, int version, AvatarBodyPart bodyPart)
        {
            if (gender == Gender.all)
                gender = Gender.female;
            
            return string.Format(bodyPartsBaseURL, style.ToString(), gender.ToString(), version, bodyPart.ToString());
        }

        public static Guid GetDefaultAssetID(Gender gender, Style style, string key)
        {
            if (gender == Gender.all)
                gender = Gender.female;

            return new Guid(defaultAssetIds[key][gender][style]);
        }
    }
}
