using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Abot2.Crawler;
using Abot2.Poco;
using EELauncher.Views;

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
    
    public static HttpResponseMessage GetRequest(Uri uri) {
        using (HttpClient httpClient = new())
            return httpClient.GetAsync(uri).Result;
    }

    public static string PostRequest(string uri, IEnumerable<KeyValuePair<string, string>> data) {
        using (HttpClient httpClient = new()) {
            HttpContent content = new FormUrlEncodedContent(data);
            HttpResponseMessage responseMessage = httpClient.PostAsync(uri, content).Result;

            return responseMessage.Content.ReadAsStringAsync().Result;
        }
    }

    public static void DownloadFile(Uri uri, string path) {
        HttpResponseMessage response = GetRequest(uri);

        try {
            using (FileStream fileStream = new(path, FileMode.CreateNew))
                response.Content.CopyToAsync(fileStream);
        } catch (IOException) {
#if DEBUG
            Trace.WriteLine($"File {response.RequestMessage!.RequestUri} already exists");
#endif
        }
    }
}
