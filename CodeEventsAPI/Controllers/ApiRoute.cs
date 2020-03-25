using System;
using System.Net.Http;
using CodeEventsAPI.Models;
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

  public struct ApiLinks {
    public readonly Func<CodeEvent, ApiRoute> GetCodeEvent;
    public readonly Func<CodeEvent, ApiRoute> JoinCodeEvent;
    public readonly Func<CodeEvent, ApiRoute> CancelCodeEvent;

    public readonly Func<CodeEvent, ApiRoute> GetMembers;
    public readonly Func<CodeEvent, ApiRoute> LeaveCodeEvent;
    public readonly Func<Member, ApiRoute> RemoveMember;


    internal ApiLinks(string entrypoint) {
      // TODO: worth refactoring to use this?
      // Func<string, string> buildEndpoint = endpoint =>
      //   $"{entrypoint}/${endpoint}";

      GetCodeEvent = codeEvent => new ApiRoute(
        $"{entrypoint}/{codeEvent.Id}",
        HttpMethod.Get
      );

      CancelCodeEvent = codeEvent => new ApiRoute(
        $"{entrypoint}/{codeEvent.Id}",
        HttpMethod.Delete
      );

      JoinCodeEvent = codeEvent => new ApiRoute(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Post
      );

      GetMembers = codeEvent => new ApiRoute(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Get
      );

      LeaveCodeEvent = codeEvent => new ApiRoute(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Delete
      );

      RemoveMember = member => new ApiRoute(
        $"{entrypoint}/{member.CodeEvent.Id}/members/{member.Id}",
        HttpMethod.Delete
      );
    }
  }
}
