using System;
using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public enum MemberRole {
    Owner,
    Member,
  }

  public class Member {
    public long Id { get; set; }
    public MemberRole Role { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }

    public long CodeEventId { get; set; }
    public CodeEvent CodeEvent { get; set; }

    public MemberDto ToDto() {
      return new MemberDto(this);
    }
  }

  public class MemberDto {
    public string Email { get; }
    public string Role { get; }

    public MemberDto(Member member) {
      Email = member.User.Email;
      Role = Enum.GetName(typeof(MemberRole), member.Role);
    }
  }
}