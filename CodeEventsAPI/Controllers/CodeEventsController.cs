using System.Net.Mime;
using CodeEventsAPI.Models;
using CodeEventsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route(Entrypoint)]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class CodeEventsController : ControllerBase {
    public const string Entrypoint = "/api/events";

    public static readonly CodeEventResourceLinks ResourceLinks =
      new CodeEventResourceLinks(Entrypoint);

    private readonly CodeEventService _codeEventService;

    public CodeEventsController(CodeEventService codeEventService) {
      _codeEventService = codeEventService;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(
      OperationId = "GetCodeEvents",
      Summary = "Retrieve all Code Events",
      Description = "Publicly available",
      Tags = new[] { "CodeEvent", "Public" }
    )]
    [SwaggerResponse(
      200,
      "List of public Code Event data",
      Type = typeof(PublicCodeEventDto)
    )]
    public ActionResult GetCodeEvents() {
      return Ok(_codeEventService.GetAllCodeEvents());
    }

    [HttpPost]
    [SwaggerOperation(
      OperationId = "CreateCodeEvent",
      Summary = "Create a new Code Event",
      Description = "Requires authentication",
      Tags = new[] { "CodeEvent", "Protected" }
    )]
    [SwaggerResponse(
      201,
      "Returns new public Code Event data",
      Type = typeof(PublicCodeEventDto)
    )]
    [SwaggerResponse(400, "Invalid or missing Code Event data", Type = null)]
    public ActionResult CreateCodeEvent(
      [FromBody, SwaggerParameter("New Code Event data", Required = true)]
      NewCodeEventDto newCodeEvent
    ) {
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
    [SwaggerOperation(
      OperationId = "GetCodeEvent",
      Summary = "Retrieve Code Event data",
      Description = "Requires an authenticated Member of the Code Event",
      Tags = new[] { "CodeEvent", "Protected", "Members Only" }
    )]
    [SwaggerResponse(
      200,
      "Complete Code Event data with links available to the requesting Member's Role",
      Type = typeof(MemberCodeEventDto)
    )]
    [SwaggerResponse(404, "Code Event not found", Type = null)]
    [SwaggerResponse(403, "Not a Member of the Code Event", Type = null)]
    public ActionResult GetCodeEvent(
      [FromRoute, SwaggerParameter("The ID of the Code Event", Required = true)]
      long codeEventId
    ) {
      var userIsMember =
        _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      var codeEventDto =
        _codeEventService.GetCodeEventById(codeEventId, HttpContext.User);
      if (codeEventDto == null) return NotFound();

      return Ok(codeEventDto);
    }

    // TODO: swagger
    [HttpDelete]
    [Route("{codeEventId}")]
    public ActionResult CancelCodeEvent([FromRoute] long codeEventId) {
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
