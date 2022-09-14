using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;

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
    
    /// <summary>
    /// Делает HTTP GET запрос
    /// </summary>
    /// <param name="uri">Адрес, по которому необходимо отправить GET запрос</param>
    /// <returns>Асинхронный ответ на GET запрос</returns>
    public static string GetRequest(string uri) {
        using (HttpClient httpClient = new())
            return httpClient.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
    }

    /// <summary>
    /// Делает HTTP POST запрос
    /// </summary>
    /// <param name="uri">Адрес, по которому необходимо отправить запрос</param>
    /// <param name="data">Данные для отправки</param>
    /// <returns>Ответ на запрос</returns>
    public static string PostRequest(string uri, IEnumerable<KeyValuePair<string, string>> data) {
        using (HttpClient httpClient = new()) {
            HttpContent xd = new FormUrlEncodedContent(data);
            HttpResponseMessage responseMessage = httpClient.PostAsync(uri, xd).Result;
            
            return responseMessage.Content.ReadAsStringAsync().Result;
        }
    }
}
