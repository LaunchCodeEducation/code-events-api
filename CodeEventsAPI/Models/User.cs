using System.Collections.Generic;
using System.Security.Claims;

namespace CodeEventsAPI.Models {
  public class User {
    public User() { }

    public User(ClaimsPrincipal authedUser) {
      Username = authedUser.Identity.Name;
      Email = authedUser.FindFirst(ClaimTypes.Email).Value;
      AzureOId = authedUser.FindFirstValue(
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
      );
    }

    // application-side unique ID for associations
    public long Id { get; set; }

    // azure provider unique ID
    public string AzureOId { get; set; }

    // TODO: check for unique
    public string Username { get; set; }

    public string Email { get; set; }

    public List<Member> Memberships { get; set; }
  }
}
