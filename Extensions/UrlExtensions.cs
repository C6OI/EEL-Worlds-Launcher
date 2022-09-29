using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;
using Serilog;

namespace EELauncher.Extensions;

public static class UrlExtensions {
    static readonly ILogger Logger = Log.Logger.ForType(typeof(UrlExtensions));
    
    public static void OpenUrl(this string url) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) Process.Start("xdg-open", url);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) Process.Start("open", url);
        else {
            Logger.Error($"Can't open the link {url}");
            return;
        }
        
        Logger.Information($"Opened the link {url}");
    }
    
    public static async Task<HttpResponseMessage> GetRequest(Uri uri) {
        using (HttpClient httpClient = new())
            return await httpClient.GetAsync(uri);
    }

    public static string PostRequest(string uri, IEnumerable<KeyValuePair<string, string>> data) {
        using (HttpClient httpClient = new()) {
            HttpContent content = new FormUrlEncodedContent(data);
            HttpResponseMessage responseMessage = httpClient.PostAsync(uri, content).Result;

            return responseMessage.Content.ReadAsStringAsync().Result;
        }
    }

    public static async Task DownloadFile(Uri uri, string path) {
        string decodedPath = HttpUtility.UrlDecode(path);
        
        if (File.Exists(decodedPath)) return;
        
        HttpResponseMessage response = await GetRequest(uri);
        string fileName = Path.GetFileName(decodedPath);

        try {
            await using (FileStream fileStream = new(decodedPath, FileMode.CreateNew)) {
                await response.Content.CopyToAsync(fileStream);
                
#if DEBUG
                Logger.Debug($"Downloaded file {fileName} from {uri}");
#endif
            }
        } catch (Exception e) {
            Logger.Error($"Exception while downloading file {fileName} from {uri}: {e}");
        }
    }
}
