using System.Net.Mime;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// TODO: use oid
// var oid = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

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
      var codeEvent = _codeEventService.RegisterCodeEvent(newCodeEvent,
        HttpContext.User);

      return CreatedAtAction(nameof(GetCodeEvent),
        new {id = codeEvent.Id},
        codeEvent);
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult GetCodeEvent(long id) {
      var codeEvent = _codeEventService.GetCodeEventById(id);
      if (codeEvent == null) return NotFound();

      return Ok(codeEvent);
    }

    // TODO: RBAC on authed user (middleware annotation or member service)
    // restrict what data / MemberDto data is included based on authed user role
    [HttpGet]
    [Route("{id}/members")]
    public ActionResult GetMembers(long id) {
      var codeEventMembers = _codeEventService.GetAllMembers(id);
      if (codeEventMembers == null) return NotFound();

      return Ok(codeEventMembers);
    }

    [HttpPost]
    [Route("{id}/members")]
    public ActionResult CreateMember(long id, HttpContext request) {
      var userCanRegister = _codeEventService.CanUserRegisterAsMember(id,
        HttpContext.User);
      if (!userCanRegister) return Forbid();

      // TODO: afterware for setting cookie acccess to /api/events/{id}?
      _codeEventService.RegisterMember(id, HttpContext.User);

      return NoContent();
    }
  }
}
