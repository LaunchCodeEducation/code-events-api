using System;
using System.Net.Http;
using CodeEventsAPI.Models;
using Microsoft.Extensions.Configuration;

namespace CodeEventsAPI.Controllers {
  public class ResourceLink {
    public string Href { get; }

    public HttpMethod Method { get; }

    public ResourceLink(string path, HttpMethod method) {
      Method = method;
      Href = $"{ServerConfig.Origin}{path}";
    }
  }

  public struct ResourceLinks {
    public readonly Func<CodeEvent, ResourceLink> GetCodeEvent;
    public readonly Func<CodeEvent, ResourceLink> JoinCodeEvent;
    public readonly Func<CodeEvent, ResourceLink> CancelCodeEvent;

    public readonly Func<CodeEvent, ResourceLink> GetMembers;
    public readonly Func<CodeEvent, ResourceLink> LeaveCodeEvent;
    public readonly Func<Member, ResourceLink> RemoveMember;


    internal ResourceLinks(string entrypoint) {
      // TODO: worth refactoring to use this?
      // Func<string, string> buildEndpoint = endpoint =>
      //   $"{entrypoint}/${endpoint}";

      GetCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}",
        HttpMethod.Get
      );

      CancelCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}",
        HttpMethod.Delete
      );

      JoinCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Post
      );

      GetMembers = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Get
      );

      LeaveCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Delete
      );

      RemoveMember = member => new ResourceLink(
        $"{entrypoint}/{member.CodeEvent.Id}/members/{member.Id}",
        HttpMethod.Delete
      );
    }
  }
}
