using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using EELauncher.Data;
using Serilog;

namespace EELauncher.Extensions;

public static class UrlExtensions {
    static readonly ILogger Logger = Log.Logger.ForType(typeof(UrlExtensions));
    
    public static async Task<BaseData> JsonHttpRequest(string uri, HttpMethod httpMethod, Dictionary<string, string>? urlData) {
        HttpRequestMessage httpRequestMessage = new() {
            Method = httpMethod,
            RequestUri = new Uri(uri),
            Headers = {
                { HttpRequestHeader.Accept.ToString(), "application/json" },
                { HttpRequestHeader.ContentType.ToString(), "application/json" }
            }
        };

        if (urlData != null) httpRequestMessage.Content = new FormUrlEncodedContent(urlData);

        using (HttpClient httpClient = new()) {
            HttpResponseMessage responseMessage = await httpClient.SendAsync(httpRequestMessage);

            BaseData responseData = new() {
                IsOk = responseMessage.IsSuccessStatusCode,
                Data = responseMessage.Content
            };

            return responseData;
        }
    }

    public static async Task DownloadFile(Uri uri, string path) {
        string decodedPath = HttpUtility.UrlDecode(path);
        
        if (File.Exists(decodedPath)) return;

        BaseData response = await JsonHttpRequest(uri.ToString(), HttpMethod.Get, null);
        
        string fileName = Path.GetFileName(decodedPath);

        try {
            await using (FileStream fileStream = new(decodedPath, FileMode.CreateNew)) {
                await response.Data.CopyToAsync(fileStream);
                
#if DEBUG
                Logger.Debug($"Downloaded file {fileName} from {uri}");
#endif
            }
        } catch (Exception e) {
            Logger.Error($"Exception while downloading file {fileName} from {uri}: {e}");
        }
    }
}
