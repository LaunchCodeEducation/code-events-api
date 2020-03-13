using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public enum MemberRole {
    Owner,
    Member,
  }

  public class Member {
    public long Id { get; set; }
    public long UserId { get; set; }
    public long CodeEventId { get; set; }
    public MemberRole Role { get; set; }

    public User User { get; set; }
    public CodeEvent CodeEvent { get; set; }
  }
}