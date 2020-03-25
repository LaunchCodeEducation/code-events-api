using System;
using System.Net.Http;
using System.Net.Mime;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
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

  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class CodeEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    public static readonly ApiLinks Routes = new ApiLinks(Entrypoint);

    private readonly CodeEventService _codeEventService;

    public CodeEventsController(CodeEventService codeEventService) {
      _codeEventService = codeEventService;
    }

    [HttpGet]
    [AllowAnonymous]
    public ActionResult GetCodeEvents() {
      return Ok(_codeEventService.GetAllCodeEvents());
    }

    [HttpPost]
    public ActionResult CreateCodeEvent(NewCodeEventDto newCodeEvent) {
      var codeEvent = _codeEventService.RegisterCodeEvent(
        newCodeEvent,
        HttpContext.User
      );

      return CreatedAtAction(
        nameof(GetCodeEvent),
        new { codeEventId = codeEvent.Id }
      );
    }

    [HttpGet]
    [Route("{codeEventId}")]
    public ActionResult GetCodeEvent(long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return Forbid();

      var codeEventDto =
        _codeEventService.GetCodeEventById(codeEventId, HttpContext.User);
      if (codeEventDto == null) return NotFound();

      return Ok(codeEventDto);
    }

    [HttpGet]
    [Route("{codeEventId}/members")]
    public ActionResult GetMembers(long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return Forbid();

      return Ok(
        _codeEventService.GetMembersList(codeEventId, HttpContext.User)
      );
    }

    [HttpPost]
    [Route("{codeEventId}/members")]
    public ActionResult JoinEvent(long codeEventId) {
      var userCanRegister =
        _codeEventService.CanUserRegisterAsMember(
          codeEventId,
          HttpContext.User
        );

      if (!userCanRegister) return BadRequest();

      _codeEventService.JoinCodeEvent(codeEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [Route("{codeEventId}/members")]
    public ActionResult LeaveCodeEvent(long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return Forbid();

      _codeEventService.LeaveCodeEvent(codeEventId, HttpContext.User);

      return NoContent();
    }

    /**
     * checks:
     * - code event exists
     * - if memberId not null-> authed user is owner
     * - if memberId null -> authed user is a member
     */
    [HttpDelete]
    [Route("{codeEventId}/members/{memberId}")]
    public ActionResult RemoveMember(long codeEventId, long memberId) {
      var isOwner = _codeEventService.IsUserAnOwner(
        codeEventId,
        HttpContext.User
      );
      if (!isOwner) return Forbid();

      var memberExists = _codeEventService.DoesMemberExist(memberId);
      if (!memberExists) return NotFound();

      _codeEventService.RemoveMember(memberId);

      return NoContent();
    }

    [HttpDelete]
    [Route("{codeEventId}")]
    public ActionResult CancelCodeEvent(long codeEventId) {
      var isOwner = _codeEventService.IsUserAnOwner(
        codeEventId,
        HttpContext.User
      );
      if (!isOwner) return Forbid();

      _codeEventService.CancelCodeEvent(codeEventId);

      return NoContent();
    }
  }
}
