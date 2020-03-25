using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace CodeEventsAPI.Controllers {
  public class ApiRoute {
    public string Path { get; }
    public HttpMethod Method { get; }

    public ApiRoute(string path, HttpMethod method) {
      Path = $"{ServerConfig.Origin}{path}";
      Method = method;
    }
  }
}
