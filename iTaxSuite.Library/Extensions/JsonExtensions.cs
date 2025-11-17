using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iTaxSuite.Library.Extensions
{
    public class IgnorePropertiesContractResolver : DefaultContractResolver
    {
        private readonly string[] propertiesToIgnore;

        public IgnorePropertiesContractResolver(params string[] propertiesToIgnore)
        {
            this.propertiesToIgnore = propertiesToIgnore;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (propertiesToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = instance => false;
            }

            return property;
        }
    }

    public static class JsonExtensions
    {
        public static string JsonCompact(this string strJson, bool removeCarriages = true)
        {
            if (string.IsNullOrWhiteSpace(strJson))
                return strJson;
            if (removeCarriages)
                strJson = strJson.Replace(@"\r\n", " ");
            var obj = JsonConvert.DeserializeObject(strJson);
            string jsonString = JsonConvert.SerializeObject(obj, Formatting.None);
            return jsonString;
        }

        public static bool JsonIsValid(this string strJson)
        {
            if (string.IsNullOrWhiteSpace(strJson))
                return false;

            strJson = strJson.Trim();
            if ((strJson.StartsWith("{") && strJson.EndsWith("}")) || //For object
                (strJson.StartsWith("[") && strJson.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strJson);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    UI.Error(jex, jex.GetBaseException().Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    UI.Error(ex, ex.GetBaseException().ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string JsonRepair(this string strJson, out bool isValid)
        {
            if (string.IsNullOrWhiteSpace(strJson))
            {
                isValid = false;
                return strJson;
            }

            var builder = new StringBuilder();
            Stack<char> stack = new Stack<char>();

            foreach (char c in strJson)
            {
                builder.Append(c);

                if (c == '{' || c == '[')
                {
                    stack.Push(c); // Get left
                }
                // Compare to right:
                else if (c == '}' && stack.Count != 0 && stack.Peek() == '{')
                {
                    stack.Pop();
                }
                else if (c == ']' && stack.Count != 0 && stack.Peek() == '[')
                {
                    stack.Pop();
                }
            }

            if (stack.Any())
            {
                Console.WriteLine($"> {string.Join(',', stack)}");
                foreach (char c in stack.ToArray())
                {
                    if (c == '{')
                    {
                        builder.Append('}');
                        stack.Pop();
                    }
                    else if (c == '[')
                    {
                        builder.Append(']');
                        stack.Pop();
                    }
                }
                Console.WriteLine($">>{string.Join(',', stack)}");
            }

            isValid = !stack.Any();
            return builder.ToString();
        }

        public static bool HasEqualValue(this object objOne, object objTwo)
        {
            if (objOne is null)
                return (objTwo is null);
            if (objTwo is null)
                return (objOne is null);
            var tokenOne = JToken.Parse(JsonConvert.SerializeObject(objOne, Formatting.None));
            var tokenTwo = JToken.Parse(JsonConvert.SerializeObject(objTwo, Formatting.None));
            return JToken.DeepEquals(tokenOne, tokenTwo);
        }

        public static bool HasEqualValue(this string jsonObjOne, string jsonObjTwo)
        {
            if (string.IsNullOrWhiteSpace(jsonObjOne))
                return string.IsNullOrWhiteSpace(jsonObjTwo);
            if (string.IsNullOrWhiteSpace(jsonObjTwo))
                return string.IsNullOrWhiteSpace(jsonObjOne);
            var tokenOne = JToken.Parse(jsonObjOne);
            var tokenTwo = JToken.Parse(jsonObjTwo);
            return JToken.DeepEquals(tokenOne, tokenTwo);
        }

    }

    public class EdmDateTimeOffsetConverter : IsoDateTimeConverter
    {
        public EdmDateTimeOffsetConverter()
        {
            DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.FFF\\Z";
        }

        public EdmDateTimeOffsetConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
    public class DecimalFormatConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"{value:0.00}");
        }

        public override bool CanRead => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

}
