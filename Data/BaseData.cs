using System.Net.Http;

namespace EELauncher.Data; 

public class BaseData {
    public bool IsOk { get; set; }
    public HttpContent Data { get; set; } = null!;
}
