using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeEventsAPI.Controllers {
  [ApiController]
  [Route("/api/events")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class EventsController : ControllerBase {
    private readonly CodeEventsDBContext _context;

    public EventsController(CodeEventsDBContext context) {
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
    // TODO: [ValidateModel]
    // https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api
    public ActionResult CreateResource(NewCodeEvent newCodeEvent) {
      if (!ModelState.IsValid) {
        return BadRequest(ModelState);
      }

      var entry = _context.Events.Add(new CodeEvent());
      entry.CurrentValues.SetValues(newCodeEvent);
      _context.SaveChanges();

      var codeEvent = entry.Entity;
      return CreatedAtAction(nameof(GetResource),
        new {id = codeEvent.Id},
        codeEvent);
    }
  }
}