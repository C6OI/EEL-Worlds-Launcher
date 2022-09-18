using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Web;

namespace EELauncher.Extensions;

public static class UrlExtensions {
    public static void OpenUrl(this string url) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            Process.Start("xdg-open", url);
        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            Process.Start("open", url);
        }
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
        HttpResponseMessage response = await GetRequest(uri);

        if (File.Exists(path)) return;

        try {
            await using (FileStream fileStream = new(HttpUtility.UrlDecode(path), FileMode.CreateNew))
                await response.Content.CopyToAsync(fileStream);
        } catch (Exception) {
            Trace.WriteLine($"File already exists: {path}");
        }
    }
}
