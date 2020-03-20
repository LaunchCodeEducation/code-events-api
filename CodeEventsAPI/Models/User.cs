using System.Collections.Generic;
using System.Security.Claims;

namespace CodeEventsAPI.Models {
  public class User {
    public User() { }

    public User(ClaimsPrincipal AdB2CUser) {
      Username = AdB2CUser.Identity.Name;
      AzureOId = AdB2CUser.FindFirstValue(
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
      );
    }

    // application-side unique ID for associations
    public long Id { get; set; }

    // azure provider unique ID
    public string AzureOId { get; set; }

    // TODO: check for unique
    public string Username { get; set; }

    public List<Member> Memberships { get; set; }
  }
}
