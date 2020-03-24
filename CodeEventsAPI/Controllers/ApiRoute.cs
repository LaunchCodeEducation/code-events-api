using System.Net.Http;

namespace CodeEventsAPI.Controllers {
  public class ApiRoute {
    public string Path { get; }
    public HttpMethod Method { get; }

    public ApiRoute(string path, HttpMethod method) {
      Path = path;
      Method = method;
    }
  }
}
