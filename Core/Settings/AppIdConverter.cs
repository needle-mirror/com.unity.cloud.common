using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Common
{
    public class AppIdConverter : JsonConverter<AppId>
    {
        public override AppId ReadJson(JsonReader reader, Type objectType, AppId existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return new AppId(reader?.Value?.ToString());
        }

        public override void WriteJson(JsonWriter writer, AppId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
