using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// TODO: use oid
// var oid = HttpContext.User.FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier");

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route("/api/events")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class ApiController : ControllerBase {
    private readonly CodeEventsDbContext _context;

    public ApiController(CodeEventsDbContext context) {
      _context = context;
    }

    [HttpGet]
    public ActionResult GetCodeEvents() {
      IEnumerable<CodeEvent> codeEvents = _context.CodeEvents.ToList();
      return Ok(codeEvents);
    }

    [HttpPost]
    public ActionResult CreateCodeEvent(NewCodeEventDto newCodeEvent) {
      var entry = _context.CodeEvents.Add(new CodeEvent());
      entry.CurrentValues.SetValues(newCodeEvent);
      _context.SaveChanges();

      var codeEvent = entry.Entity;
      return CreatedAtAction(nameof(GetCodeEvent),
        new {id = codeEvent.Id},
        codeEvent);
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult GetCodeEvent(long id) {
      var codeEvent = _context.CodeEvents.Find(id);
      if (codeEvent == null) return NotFound();

      return Ok(codeEvent);
    }

    [HttpGet]
    [Route("{id}/members")]
    public ActionResult GetMembers(long id) {
      var codeEvent = _context.CodeEvents
        .Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .Single(ce => ce.Id == id);

      if (codeEvent == null) return NotFound();

      return Ok(codeEvent.Members.Select(member => member.ToDto()));
    }

    [HttpPost]
    [Route("{id}/members")]
    public ActionResult CreateMember(long id, HttpContext request) {
      var codeEvent = _context.CodeEvents.Find(id);
      if (codeEvent == null) return NotFound();
      // TODO: pull user data from auth (jwt/cookie?)
      // integrate with AD or simulate for now?

      return Ok();
    }
  }
}