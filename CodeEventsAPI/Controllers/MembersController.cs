using System.Net.Mime;
using CodeEventsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class MembersController : ControllerBase {
    public const string Entrypoint = "/api/events/{codeEventId}/members";

    private readonly CodeEventService _codeEventService;

    public static readonly MemberResourceLinks ResourceLinks =
      new MemberResourceLinks(Entrypoint);

    public MembersController(CodeEventService codeEventService) {
      _codeEventService = codeEventService;
    }

    [HttpGet]
    public ActionResult GetMembers([FromRoute] long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      return Ok(
        _codeEventService.GetMembersList(codeEventId, HttpContext.User)
      );
    }

    [HttpPost]
    public ActionResult JoinEvent([FromRoute] long codeEventId) {
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
    public ActionResult LeaveCodeEvent([FromRoute] long codeEventId) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      var userIsOwner =
        _codeEventService.IsUserAnOwner(codeEventId, HttpContext.User);
      if (userIsOwner) return BadRequest();

      _codeEventService.LeaveCodeEvent(codeEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [Route("{memberId}")]
    public ActionResult RemoveMember(
      [FromRoute] long codeEventId,
      [FromRoute] long memberId
    ) {
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
  }
}
