using System;
using CodeEventsAPI.Controllers;

namespace CodeEventsAPI.Models {
  public enum MemberRole {
    Owner, // event.view, event.edit, event.delete, event.member.view, event.member.remove
    Member // event.view, event.member.view:username, event.member.remove:self
  }

  public class Member {
    public Member() { }

    private Member(CodeEvent codeEvent, User user, MemberRole role) {
      this.CodeEvent = codeEvent;
      this.User = user;
      this.Role = role;
    }

    private Member(long codeEventId, long userId, MemberRole role) {
      CodeEventId = codeEventId;
      UserId = userId;
      Role = role;
    }

    public static Member CreateEventOwner(CodeEvent codeEvent, User member) {
      return new Member(codeEvent, member, MemberRole.Owner);
    }

    public static Member CreateEventMember(long codeEventId, long userId) {
      return new Member(codeEventId, userId, MemberRole.Member);
    }

    public long Id { get; set; }
    public MemberRole Role { get; set; }

    public long UserId { get; set; }
    public User User { get; set; }

    public long CodeEventId { get; set; }
    public CodeEvent CodeEvent { get; set; }

    public MemberDto ToDto(Member requestingMember) {
      return requestingMember.Role switch {
        MemberRole.Owner => MemberDto.ForOwner(this),
        MemberRole.Member => MemberDto.ForMember(this),
        _ => null
      };
    }
  }

  public class MemberDto {
    private MemberDto(Member member) {
      Email = null;
      Links = null;
      Username = member.User.Username;
      Role = Enum.GetName(typeof(MemberRole), member.Role);
    }

    public static MemberDto ForMember(Member member) => new MemberDto(member);

    public static MemberDto ForOwner(Member member) {
      var memberDtoBase = new MemberDto(member);
      memberDtoBase.Email = member.User.Email;
      memberDtoBase.Links = new {
        Remove = CodeEventsController.ResourceLinks.RemoveMember(member),
      };

      return memberDtoBase;
    }

    public string Role { get; }
    public string Username { get; }
    public string Email { get; private set; }
    public object Links { get; private set; }
  }
}
