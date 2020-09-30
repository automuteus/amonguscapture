using System;
using System.IO;
using Config.Net;

namespace AmongUsCapture
{
    public static class Settings
    {
        public static string StorageLocation = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\AmongUsCapture");

        public static ConsoleInterface conInterface;
        //Global persistent settings that are saved to a json file. Limited Types
        public static IPersistentSettings PersistentSettings = new ConfigurationBuilder<IPersistentSettings>().UseJsonFile(Path.Join(StorageLocation, "Settings.json")).Build();
    }


    public interface IPersistentSettings
    {
        //Types allowed: bool, double, int, long, string, TimeSpan, DateTime, Uri, Guid
        //DateTime is always converted to UTC
        [Option(Alias = "Host", DefaultValue = "http://localhost:8123")]
        string host { get; set; }
        
    }
}