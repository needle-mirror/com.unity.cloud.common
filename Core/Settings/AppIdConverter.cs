using System;
using Newtonsoft.Json;

namespace Unity.Cloud.Common
{
    /// <summary>
    /// Custom <see cref="JsonConverter"/> for <see cref="AppId"/>.
    /// </summary>
    public class AppIdConverter : JsonConverter<AppId>
    {
        /// <inheritdoc/>
        public override AppId ReadJson(JsonReader reader, Type objectType, AppId existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            return new AppId(reader?.Value?.ToString());
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, AppId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}
