#region License
// MIT License
//
// Copyright (c) 2018 Thor Brigsted
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to
// whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace UnionAvatars.Utils
{
    /// <summary>
    /// Converts from float array to Quaternion during deserialization, and back.
    /// Compensates for differing coordinate systems as well.
    /// </summary>
    [Preserve]
    public class QuaternionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Quaternion q = (Quaternion)value;
            writer.WriteStartArray();
            writer.WriteValue(q.x);
            writer.WriteValue(-q.y);
            writer.WriteValue(-q.z);
            writer.WriteValue(q.w);
            writer.WriteEndArray();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            float[] floatArray = serializer.Deserialize<float[]>(reader);
            return new Quaternion(floatArray[0], -floatArray[1], -floatArray[2], floatArray[3]);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Quaternion);
        }
    }

    [Preserve]
    public class VersionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            int v = (int)value;
            writer.WriteValue("v" + v);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            string v = (string)reader.Value;
            return (int)(v[1] - '0');
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }
    }

    [Preserve]
    public class ColorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Color c = (Color)value;
            writer.WriteStartObject();
            writer.WritePropertyName("r");
            writer.WriteValue(c.r);
            writer.WritePropertyName("g");
            writer.WriteValue(c.g);
            writer.WritePropertyName("b");
            writer.WriteValue(c.b);
            writer.WriteEndObject();
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            JObject jObject = JObject.Load(reader);
            Color color = new Color();
            color.r = (float)jObject["r"];
            color.g = (float)jObject["g"];
            color.b = (float)jObject["b"];
            return color;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(int);
        }
    }

    class TolerantStringEnumConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            Type type = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;
            return type.IsEnum;
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer
        )
        {
            bool isNullable = IsNullableType(objectType);
            Type enumType = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            string[] names = Enum.GetNames(enumType);

            if (reader.TokenType == JsonToken.String)
            {
                string enumText = reader.Value.ToString();

                if (!string.IsNullOrEmpty(enumText))
                {
                    string match = names
                        .Where(n => string.Equals(n, enumText, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (match != null)
                    {
                        return Enum.Parse(enumType, match);
                    }
                }
            }
            else if (reader.TokenType == JsonToken.Integer)
            {
                int enumVal = Convert.ToInt32(reader.Value);
                int[] values = (int[])Enum.GetValues(enumType);
                if (values.Contains(enumVal))
                {
                    return Enum.Parse(enumType, enumVal.ToString());
                }
            }

            if (!isNullable)
            {
                string defaultName = names
                    .Where(n => string.Equals(n, "Unknown", StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                if (defaultName == null)
                {
                    defaultName = names.First();
                }

                return Enum.Parse(enumType, defaultName);
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        private bool IsNullableType(Type t)
        {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
