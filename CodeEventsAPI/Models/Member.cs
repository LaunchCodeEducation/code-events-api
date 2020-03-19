using System;

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
      // TODO: check for owner? service level?
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

    public MemberDto ToDto() {
      return new MemberDto(this);
    }
  }

  public class MemberDto {
    public MemberDto(Member member) {
      Username = member.User.Username;
      Role = Enum.GetName(typeof(MemberRole), member.Role);
    }

    public string Username { get; }
    public string Role { get; }
  }
}
