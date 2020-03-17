using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public class User {
    // application-side unique ID for associations
    public long Id { get; set; }

    // azure provider unique ID
    public string AzureOId { get; set; }

    public string Username { get; set; }

    public List<Member> Memberships { get; set; }
  }
}