using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EELauncher.Extensions; 

public static class UrlExtensions {
    public static void OpenUrl(this string url) {
        try {
            Process.Start(url);
        } catch {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))  {
                Process.Start("xdg-open", url);
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))  {
                Process.Start("open", url);
            } else {
                throw;
            }
        }
    }
}
