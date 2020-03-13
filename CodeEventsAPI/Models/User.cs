// TODO: how to associate User with Event
// EventMember class?

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CodeEventsAPI.Models {
  public class User {
    public long Id { get; set; }

    public string Email { get; set; }

    // TODO: hash password on set
    public string Password { get; set; }

    // virtual marks as lazy loaded
    public virtual List<Member> Memberships { get; set; }

    // TODO: convenience method for direct access to role + event objects
    // public IEnumerable<Object> Events {
    //   get => Memberships.Select(membership => new {
    //     role = membership.Role, 
    //     event = membership.Event
    //   });
    // }
  }

  public class NewUserDto {
    [Required] [EmailAddress] public string Email { get; set; }

    [Required]
    [StringLength(64,
      MinimumLength = 10,
      ErrorMessage = "Password must be between 10 and 64 characters")]
    public string Password { get; set; }

    [Required]
    [Compare(nameof(Password),
      ErrorMessage = "Password values do not match")]
    [StringLength(64,
      MinimumLength = 10,
      ErrorMessage = "Password must be between 10 and 64 characters")]
    public string ConfirmPassword { get; set; }
  }
}