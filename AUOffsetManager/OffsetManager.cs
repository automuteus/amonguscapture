using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AUOffsetManager
{
    public class OffsetManager
    {
        public static int GameMemReaderVersion = 1; //GameMemReader should update this.
        private Dictionary<string, GameOffsets> OffsetIndex = new Dictionary<string, GameOffsets>();
        private Dictionary<string, GameOffsets> LocalOffsetIndex = new Dictionary<string, GameOffsets>();
        public string indexURL;
        private string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\index.json");
        private string StorageLocationCache = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture\\indexCache.json");

        public Task indexTask;
        public OffsetManager(string indexURL = "")
        {
            this.indexURL = indexURL;
            if (File.Exists(StorageLocation))
            {
                 LocalOffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(File.ReadAllText(StorageLocation));
                 if (LocalOffsetIndex is null)
                 {
                     LocalOffsetIndex = new Dictionary<string, GameOffsets>();
                 }
            }

            indexTask = RefreshIndex();
        }
        public async Task RefreshIndex()
        {
            if (indexURL == "")
            {
                OffsetIndex = new Dictionary<string, GameOffsets>();
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                try
                {
                    var json = await httpClient.GetStringAsync(indexURL);
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
                    await using StreamWriter sw = File.CreateText(StorageLocationCache);
                    await sw.WriteAsync(JsonConvert.SerializeObject(OffsetIndex, Formatting.Indented));
                }
                catch (Exception e)
                {
                    indexURL = "https://raw.githubusercontent.com/denverquane/amonguscapture/master/Offsets.json";
                    var json = await httpClient.GetStringAsync(indexURL);
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(json);
                    await using StreamWriter sw = File.CreateText(StorageLocationCache);
                    await sw.WriteAsync(JsonConvert.SerializeObject(OffsetIndex, Formatting.Indented));
                    Console.WriteLine(e);
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                if (File.Exists(StorageLocationCache))
                {
                    OffsetIndex = JsonConvert.DeserializeObject<Dictionary<string, GameOffsets>>(await File.ReadAllTextAsync(StorageLocationCache));
                }
                Console.WriteLine("If you are reading this that means that the site is down, and you have never used our program before. If github still exists in the future, try again in 30 minutes. - Carbon ");
            }
            
        }

        public GameOffsets FetchForHash(string sha256Hash)
        {
            indexTask.Wait();
            if (LocalOffsetIndex.ContainsKey(sha256Hash))
            {
                Console.WriteLine($"Loaded offsets: {LocalOffsetIndex[sha256Hash].Description}");
                return LocalOffsetIndex[sha256Hash];
            }
            else
            {
                var offsets = OffsetIndex.ContainsKey(sha256Hash) ? OffsetIndex[sha256Hash] : null;
                if (offsets is not null)
                {
                    Console.WriteLine($"Loaded offsets: {OffsetIndex[sha256Hash].Description}");
                }
                return offsets;
            }
                
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
        public string Description = "";

        public int AmongUsClientOffset { get; set; }

        public int GameDataOffset { get; set; }

        public int MeetingHudOffset { get; set; }

        public int GameStartManagerOffset { get; set; }

        public int HudManagerOffset { get; set; }

        public int ServerManagerOffset { get; set; }

        public int TempDataOffset { get; set; }
        
        public int GameOptionsOffset { get; set; }

        public int[] MeetingHudPtr { get; set; }
        public int[] MeetingHudCachePtrOffsets { get; set; }
        public int[] MeetingHudStateOffsets { get; set; }
        public int[] GameStateOffsets { get; set; }
        public int[] AllPlayerPtrOffsets { get; set; }
        public int[] AllPlayersOffsets { get; set; }
        public int[] PlayerCountOffsets { get; set; }
        public int[] ExiledPlayerIdOffsets { get; set; }
        public int[] RawGameOverReasonOffsets { get; set; }
        public int[] WinningPlayersPtrOffsets { get; set; }
        public int[] WinningPlayersOffsets { get; set; }
        public int[] WinningPlayerCountOffsets { get; set; }
        public int[] GameCodeOffsets { get; set; }
        public int[] PlayRegionOffsets { get; set; }
        public int[] PlayMapOffsets { get; set; }
        public int[] StringOffsets { get; set; }
        public bool isEpic { get; set; }
        public int AddPlayerPtr { get; set; }
        public int PlayerListPtr { get; set; }
        
        public PlayerInfoStructOffsets PlayerInfoStructOffsets { get; set; }
        public WinningPlayerDataStructOffsets WinningPlayerDataStructOffsets { get; set; }
        
    }

    public class PlayerInfoStructOffsets {
        public int PlayerIDOffset { get; set; }
        public int PlayerNameOffset { get; set; }
        public int ColorIDOffset { get; set; }
        public int HatIDOffset { get; set; }
        public int PetIDOffset { get; set; }
        public int SkinIDOffset { get; set; }
        public int DisconnectedOffset { get; set; }
        public int TasksOffset { get; set; }
        public int ImposterOffset { get; set; }
        public int DeadOffset { get; set; }
        public int ObjectOffset { get; set; }
    }

    public class WinningPlayerDataStructOffsets {
        public int NameOffset { get; set; }
        public int DeadOffset { get; set; }
        public int ImposterOffset { get; set; }
        public int ColorOffset { get; set; }
        public int SkinOffset { get; set; }
        public int HatOffset { get; set; }
        public int PetOffset { get; set; }
        public int IsYouOffset { get; set; }
    }

}
