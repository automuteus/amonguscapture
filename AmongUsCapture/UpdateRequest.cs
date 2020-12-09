using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AmongUsCapture
{
    public partial class UpdateRequest
    {
        [JsonProperty("guildID")]
        public ulong GuildId { get; set; }

        [JsonProperty("userID")]
        public ulong UserId { get; set; }

        [JsonProperty("parameters")]
        public Parameters Parameters { get; set; }

        [JsonProperty("taskID")]
        public string TaskId { get; set; }
    }

    public partial class Parameters
    {
        [JsonProperty("deaf")]
        public bool Deaf { get; set; }

        [JsonProperty("mute")]
        public bool Mute { get; set; }
    }

    public partial class UpdateRequest
    {
        public static UpdateRequest FromJson(string json) => JsonConvert.DeserializeObject<UpdateRequest>(json, AmongUsCapture.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this UpdateRequest self) => JsonConvert.SerializeObject(self, AmongUsCapture.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
