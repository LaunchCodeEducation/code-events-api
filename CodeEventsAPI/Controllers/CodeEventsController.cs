using System.Net.Mime;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route("/api/events")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class CodeEventsController : ControllerBase {
    private readonly CodeEventService _codeEventService;

    public CodeEventsController(CodeEventService codeEventService) {
      _codeEventService = codeEventService;
    }

    [HttpGet]
    public ActionResult GetCodeEvents() {
      return Ok(_codeEventService.GetAllCodeEvents());
    }

    [HttpPost]
    public ActionResult CreateCodeEvent(NewCodeEventDto newCodeEvent) {
      var codeEventDto =
        _codeEventService.RegisterCodeEvent(newCodeEvent, HttpContext.User);

      return CreatedAtAction(
        nameof(GetCodeEvent),
        new { codeEventId = codeEventDto.Id },
        codeEventDto
      );
    }

    [HttpGet]
    [Route("{codeEventId}")]
    public ActionResult GetCodeEvent(long codeEventId) {
      var codeEventDto = _codeEventService.GetCodeEventById(codeEventId);
      if (codeEventDto == null) return NotFound();

      return Ok(codeEventDto);
    }

    // TODO: RBAC on authed user CodeEvent (middleware annotation or member service)
    // restrict what data / MemberDto data is included based on authed user role
    [HttpGet]
    [Route("{codeEventId}/members")]
    public ActionResult GetMembers(long codeEventId) {
      var codeEventMemberDtos = _codeEventService.GetAllMembers(codeEventId);
      if (codeEventMemberDtos == null) return NotFound();

      return Ok(codeEventMemberDtos);
    }

    [HttpPost]
    [Route("{codeEventId}/members")]
    public ActionResult CreateMember(long codeEventId) {
      var userCanRegister =
        _codeEventService.CanUserRegisterAsMember(
          codeEventId,
          HttpContext.User
        );

      if (!userCanRegister) return Forbid();

      // TODO: afterware for setting cookie acccess to /api/events/{codeEventId}?
      _codeEventService.RegisterMember(codeEventId, HttpContext.User);

      return NoContent();
    }
  }
}
