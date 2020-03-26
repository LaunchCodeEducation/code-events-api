using System.Collections.Generic;
using System.Net.Mime;
using CodeEventsAPI.Models;
using CodeEventsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CodeEventsAPI.Controllers {
  [Authorize]
  [ApiController]
  [Route("/api/events/{codeEventId}/members")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class MembersController : ControllerBase {
    private readonly CodeEventService _codeEventService;

    // the entrypoint is still /api/events while {codeEventId} is inserted dynamically
    public static readonly MemberResourceLinks ResourceLinks =
      new MemberResourceLinks(CodeEventsController.Entrypoint);

    public MembersController(CodeEventService codeEventService) {
      _codeEventService = codeEventService;
    }

    [HttpGet]
    [SwaggerOperation(
      OperationId = "GetMembers",
      Summary = "Retrieve Members of the Code Event",
      Description = @"
Requires an authenticated Member.
The Member data will differ depending on the requesting Member's Role
",
      Tags = new[] { SwaggerConfig.RequireMemberTag, SwaggerConfig.RequireOwnerTag }
    )]
    [SwaggerResponse(
      200,
      @"
List of Members with data dependent on the requesting Member's Role.<br>
Member Role: limited data (username and role).<br>
Owner Role: complete data (username, role, email, and links.remove)
",
      Type = typeof(List<MemberDto>)
    )]
    [SwaggerResponse(403, "Not a Member of the Code Event", Type = null)]
    public ActionResult GetMembers(
      [FromRoute, SwaggerParameter("The ID of the Code Event", Required = true)]
      long codeEventId
    ) {
      var userIsMember = _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      return Ok(_codeEventService.GetMembersList(codeEventId, HttpContext.User));
    }

    [HttpPost]
    [SwaggerOperation(
      OperationId = "JoinCodeEvent",
      Summary = "Join a Code Event",
      Description = "Requires an authenticated User",
      Tags = new[] { SwaggerConfig.RequireUserTag, }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(400, "User is already a Member or Owner", Type = null)]
    public ActionResult JoinEvent(
      [FromRoute, SwaggerParameter("The ID of the Code Event", Required = true)]
      long codeEventId
    ) {
      var userCanRegister =
        _codeEventService.CanUserRegisterAsMember(codeEventId, HttpContext.User);

      if (!userCanRegister) return BadRequest();

      _codeEventService.JoinCodeEvent(codeEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [SwaggerOperation(
      OperationId = "LeaveCodeEvent",
      Summary = "Leave a Code Event",
      Description = "Requires an authenticated Member",
      Tags = new[] { SwaggerConfig.RequireMemberTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(400, "User is an Owner. (See CancelCodeEvent)", Type = null)]
    [SwaggerResponse(403, "User is not a Member", Type = null)]
    public ActionResult LeaveCodeEvent(
      [FromRoute, SwaggerParameter("The ID of the Code Event", Required = true)]
      long codeEventId
    ) {
      var userIsMember = _codeEventService.IsUserAMember(codeEventId, HttpContext.User);
      if (!userIsMember) return StatusCode(403);

      var userIsOwner = _codeEventService.IsUserAnOwner(codeEventId, HttpContext.User);
      if (userIsOwner) return BadRequest();

      _codeEventService.LeaveCodeEvent(codeEventId, HttpContext.User);

      return NoContent();
    }

    [HttpDelete]
    [Route("{memberId}")]
    [SwaggerOperation(
      OperationId = "RemoveMember",
      Summary = "Remove a Member from a Code Event",
      Description = "Requires an authenticated Owner",
      Tags = new[] { SwaggerConfig.RequireOwnerTag }
    )]
    [ProducesResponseType(204)] // suppress default swagger 200 response code
    [SwaggerResponse(204, "No content success", Type = null)]
    [SwaggerResponse(403, "User is not an Owner", Type = null)]
    [SwaggerResponse(404, "Member not found", Type = null)]
    public ActionResult RemoveMember(
      [FromRoute, SwaggerParameter("The ID of the Code Event", Required = true)]
      long codeEventId,
      [FromRoute, SwaggerParameter("The ID of the Member", Required = true)]
      long memberId
    ) {
      var isOwner = _codeEventService.IsUserAnOwner(codeEventId, HttpContext.User);
      if (!isOwner) return StatusCode(403);

      var memberExists = _codeEventService.DoesMemberExist(memberId);
      if (!memberExists) return NotFound();

      _codeEventService.RemoveMember(memberId);

      return NoContent();
    }
  }
}
