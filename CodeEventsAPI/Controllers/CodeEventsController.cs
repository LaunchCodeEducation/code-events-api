using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// TODO: abstract to APIController base class
namespace CodeEventsAPI.Controllers {
  [ApiController]
  [Route("/api/events")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class CodeEventsController : ControllerBase {
    private readonly CodeEventsDbContext _context;

    public CodeEventsController(CodeEventsDbContext context) {
      _context = context;
    }

    [HttpGet]
    public ActionResult GetCollection() {
      IEnumerable<CodeEvent> codeEvents = _context.Events.ToList();
      return Ok(codeEvents);
    }

    [HttpGet]
    [Route("{id}")]
    public ActionResult GetResource(long id) {
      var codeEvent = _context.Events.Find(id);
      if (codeEvent == null) return NotFound();

      return Ok(codeEvent);
    }

    [HttpPost]
    // TODO: abstract validation/rejection into [ValidateModel]
    // https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api
    public ActionResult CreateResource(NewCodeEventDto newCodeEvent) {
      var entry = _context.Events.Add(new CodeEvent());
      entry.CurrentValues.SetValues(newCodeEvent);
      _context.SaveChanges();

      var codeEvent = entry.Entity;
      return CreatedAtAction(nameof(GetResource),
        new {id = codeEvent.Id},
        codeEvent);
    }

    [HttpGet]
    [Route("{id}/members")]
    public ActionResult GetMembersCollection(long id) {
      var codeEvent = _context.Events.Find(id);
      if (codeEvent == null) return NotFound();

      // TODO: map Roles from numeric enum index
      return Ok(codeEvent.Members.Select(member => member.User));
    }

    [HttpPost]
    [Route("{id}/members")]
    public ActionResult CreateMemberResource(long id) {
      var codeEvent = _context.Events.Find(id);
      if (codeEvent == null) return NotFound();
      // TODO: pull user data from auth (jwt/cookie?)
      // integrate with AD or simulate for now?

      return Ok();
    }
  }
}