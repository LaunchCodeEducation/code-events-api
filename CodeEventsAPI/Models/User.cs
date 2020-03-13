// TODO: how to associate User with Event
// EventMember class?

using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public class User {
    public long Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
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