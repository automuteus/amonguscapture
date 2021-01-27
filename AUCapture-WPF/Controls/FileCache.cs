using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AUCapture_WPF.Controls
{
    public static class FileCache
    {
        public enum CacheMode
        {
            WinINet,
            Dedicated
        }

        // Record whether a file is being written.
        private static readonly Dictionary<string, bool> _isWritingFile = new Dictionary<string, bool>();
        private static readonly HttpClient _httpClient = new HttpClient();



        static FileCache()
        {
            // default cache directory - can be changed if needed from App.xaml
            AppCacheDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AmongUsCapture",
                "Cache");
            AppCacheMode = CacheMode.Dedicated;
        }



        /// <summary>
        /// Gets or sets the path to the folder that stores the cache file. Only works when AppCacheMode is
        /// CacheMode.Dedicated.
        /// </summary>
        public static string AppCacheDirectory { get; set; }

        /// <summary>
        /// Gets or sets the cache mode. WinINet is recommended, it's provided by .Net Framework and uses the Temporary Files
        /// of IE and the same cache policy of IE.
        /// </summary>
        public static CacheMode AppCacheMode { get; set; }



        public static async Task<MemoryStream> HitAsync(string url)
        {
            if (!Directory.Exists(AppCacheDirectory))
            {
                Directory.CreateDirectory(AppCacheDirectory);
            }

            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                return null;
            }

            var fileNameBuilder = new StringBuilder();
            using (var sha1 = new SHA1Managed())
            {
                var canonicalUrl = uri.ToString();
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(canonicalUrl));
                fileNameBuilder.Append(BitConverter.ToString(hash).Replace("-", "").ToLower());

                if (Path.HasExtension(canonicalUrl))
                {
                    fileNameBuilder.Append(Path.GetExtension(canonicalUrl.Split('?')[0]));
                }
            }

            var fileName = fileNameBuilder.ToString();
            var localFile = Path.Combine(AppCacheDirectory, fileName);
            var memoryStream = new MemoryStream();

            FileStream fileStream = null;
            if (!_isWritingFile.ContainsKey(fileName) && File.Exists(localFile))
            {
                await using (fileStream = new FileStream(localFile, FileMode.Open, FileAccess.Read))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }

            try
            {
                var responseStream = await _httpClient.GetStreamAsync(uri);

                if (responseStream == null)
                {
                    return null;
                }

                if (!_isWritingFile.ContainsKey(fileName))
                {
                    _isWritingFile[fileName] = true;
                    fileStream = new FileStream(localFile, FileMode.Create, FileAccess.Write);
                }

                await using (responseStream)
                {
                    var bytebuffer = new byte[1024];
                    int bytesRead;
                    do
                    {
                        bytesRead = await responseStream.ReadAsync(bytebuffer, 0, 1024);
                        if (fileStream != null)
                        {
                            await fileStream.WriteAsync(bytebuffer, 0, bytesRead);
                        }

                        await memoryStream.WriteAsync(bytebuffer, 0, bytesRead);
                    } while (bytesRead > 0);
                    if (fileStream != null)
                    {
                        await fileStream.FlushAsync();
                        fileStream.Dispose();
                        _isWritingFile.Remove(fileName);
                    }
                }
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
