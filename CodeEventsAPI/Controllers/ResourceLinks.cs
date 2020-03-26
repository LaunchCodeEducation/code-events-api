using System;
using System.Net.Http;
using CodeEventsAPI.Models;
using Microsoft.Extensions.Configuration;

namespace CodeEventsAPI.Controllers {
  public class ResourceLink {
    public string Href { get; }

    public HttpMethod Method { get; }

    internal ResourceLink(string path, HttpMethod method) {
      Method = method;
      Href = $"{ServerConfig.Origin}{path}";
    }
  }

  public struct CodeEventResourceLinks {
    public readonly Func<CodeEvent, ResourceLink> GetCodeEvent;
    public readonly Func<CodeEvent, ResourceLink> CancelCodeEvent;

    // private static Func<string, string> ConfigureEndpointBuilder(
    //   string entrypoint
    // ) => endpoint => $"{entrypoint}/${endpoint}";

    internal CodeEventResourceLinks(string entrypoint) {
      GetCodeEvent = codeEvent => new ResourceLink($"{entrypoint}/{codeEvent.Id}", HttpMethod.Get);

      CancelCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}",
        HttpMethod.Delete
      );
    }
  }

  public struct MemberResourceLinks {
    public readonly Func<CodeEvent, ResourceLink> GetMembers;
    public readonly Func<CodeEvent, ResourceLink> JoinCodeEvent;
    public readonly Func<CodeEvent, ResourceLink> LeaveCodeEvent;
    public readonly Func<Member, ResourceLink> RemoveMember;

    internal MemberResourceLinks(string entrypoint) {
      GetMembers = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Get
      );

      JoinCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Post
      );

      LeaveCodeEvent = codeEvent => new ResourceLink(
        $"{entrypoint}/{codeEvent.Id}/members",
        HttpMethod.Delete
      );

      RemoveMember = member => new ResourceLink(
        $"{entrypoint}/{member.CodeEventId}/members/{member.Id}",
        HttpMethod.Delete
      );
    }
  }
}
