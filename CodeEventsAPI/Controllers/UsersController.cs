using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
  [ApiController]
  [Route("/api/users")]
  [Consumes(MediaTypeNames.Application.Json)]
  [Produces(MediaTypeNames.Application.Json)]
  public class UsersController : ControllerBase {
    private readonly CodeEventsDbContext _context;

    public UsersController(CodeEventsDbContext context) {
      _context = context;
    }

    [HttpPost]
    public ActionResult CreateResource(NewUserDto newUserDto) {

      var entry = _context.Users.Add(new User());
      entry.CurrentValues.SetValues(newUserDto);
      _context.SaveChanges();

      return NoContent();
    }
  }
}