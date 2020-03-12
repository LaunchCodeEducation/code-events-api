using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace CodeEventsAPI.Controllers {
  [ApiController]
  [Route("/api/[controller]")]
  public class EventsController : ControllerBase {
    [HttpGet]
    public string Get() => "got it";
  }
}