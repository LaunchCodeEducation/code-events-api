using System.Net.Mime;
using CodeEventsAPI.Models;
using CodeEventsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class CodeEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    public static readonly ResourceLinks ResourceLinks =
      new ResourceLinks(Entrypoint);

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
        new { codeEventId = codeEvent.Id },
        codeEvent.ToPublicDto()
      );
    }

    [HttpGet]
    [Route("{codeEventId}")]
    public ActionResult GetCodeEvent(long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

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
      if (!userIsMember) return StatusCode(403);

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
      if (!userIsMember) return StatusCode(403);

      var userIsOwner =
        _codeEventService.IsUserAnOwner(codeEventId, HttpContext.User);
      if (userIsOwner) return BadRequest();

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
      if (!isOwner) return StatusCode(403);

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
      if (!isOwner) return StatusCode(403);

      _codeEventService.CancelCodeEvent(codeEventId);

      return NoContent();
    }
  }
}
