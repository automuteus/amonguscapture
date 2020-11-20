using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;

namespace AUOffsetManager
{
    public class OffsetManager
    {
        public static int GameMemReaderVersion = 1; //GameMemReader should update this.
        private Dictionary<string, GameOffsets> OffsetIndex = new Dictionary<string, GameOffsets>();
        private Dictionary<string, GameOffsets> LocalOffsetIndex = new Dictionary<string, GameOffsets>();
        private string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\index.json");
        public OffsetManager(string indexURL = "")
        {
            if (File.Exists(StorageLocation))
            {
                 LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
                 if (LocalOffsetIndex is null)
                 {
                     LocalOffsetIndex = new Dictionary<string, GameOffsets>();
                 }
            }
            RefreshIndex(indexURL);
        }
        public async void RefreshIndex(string indexURL)
        {
            if (indexURL == "")
            {
                OffsetIndex = new Dictionary<string, GameOffsets>();
                return;
            }
            using var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync(indexURL);
            OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
        }

        public GameOffsets FetchForHash(string sha256Hash)
        {
            return LocalOffsetIndex.ContainsKey(sha256Hash) ? LocalOffsetIndex[sha256Hash] :
                OffsetIndex.ContainsKey(sha256Hash) ? OffsetIndex[sha256Hash] : null;
        }

        public void refreshLocal()
        {
            if (File.Exists(StorageLocation))
            {
                LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
            }
        }
        public void AddToLocalIndex(string gameHash,GameOffsets offset)
        {
            using StreamWriter sw = File.CreateText(StorageLocation);
            LocalOffsetIndex[gameHash] = offset;
            var serialized = JsonConvert.SerializeObject(LocalOffsetIndex, Formatting.Indented);
            sw.Write(serialized);
        }
    }

    public class GameOffsets
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public string description = "";

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] allPlayersOffsets; //Current: {0x08}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] allPlayersPtrOffsets; //Current: {_gameOffsets.GameDataOffset, 0x5C, 0, 0x24}

        public int chatBubblePoolSize; //Current: 20

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] chatBubblesAddrOffsets; //Current: {0x8}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] chatBubblesPtrOffsets; //Current: {_gameOffsets.HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] chatBubsVersionOffsets; //Current: {0x10}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] exiledPlayerIdOffsets; //Current: {_gameOffsets.MeetingHudOffset, 0x5C, 0, 0x94, 0x08}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] gameCodeOffsets; //Current: {_gameOffsets.GameStartManagerOffset, 0x5c, 0, 0x20, 0x28}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] gameStateOffsets; //Current: {_gameOffsets.AmongUsClientOffset, 0x5C, 0, 0x64}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] meetingHudCacheOffsets; //Current: {0x8}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] meetingHudPtrOffsets; //Current: {_gameOffsets.MeetingHudOffset, 0x5C, 0}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] meetingHudStateOffsets; //Current: {0x84} 

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] messageTextOffsets; //Current: {0x20, 0x28}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] messageSenderOffsets; //Current: {0x1C, 0x28}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] numChatBubblesOffsets; //Current: {0xC}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] playerCountOffsets; //Current: {0x0C}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[]
            prevChatBubsVersionOffsets; //Current: {_gameOffsets.HudManagerOffset, 0x5C, 0, 0x28, 0xC, 0x14, 0x10}

        [JsonProperty(ItemConverterType = typeof(HexStringJsonConverter))]
        public int[] regionOffsets; //Current: {_gameOffsets.ServerManagerOffset, 0x5c, 0, 0x10, 0x8, 0x8}
    }

    public sealed class HexStringJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(int) == objectType;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue($"0x{value:x}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray || reader.TokenType == JsonToken.EndArray || reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else
            {
                if (reader.TokenType == JsonToken.Integer)
                {
                    return Int32.Parse(reader.Value.ToString());
                }
            }

            return null;

        }
    }
}