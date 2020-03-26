using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography;
using CodeEventsAPI.Controllers;
using Swashbuckle.AspNetCore.Filters;

namespace CodeEventsAPI.Models {
  public enum MemberRole {
    Owner,
    Member
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
        MemberRole.Member => MemberDto.ForMember(this),
        MemberRole.Owner => MemberDto.ForOwner(this, requestingMember),
        _ => null
      };
    }
  }

  public class MemberDto {
    internal MemberDto() { }

    private MemberDto(Member member) {
      Email = null;
      Links = new ExpandoObject();
      Username = member.User.Username;
      Role = Enum.GetName(typeof(MemberRole), member.Role);
    }

    public static MemberDto ForMember(Member member) => new MemberDto(member);

    public static MemberDto ForOwner(Member member, Member owner) {
      var memberDtoBase = new MemberDto(member);
      memberDtoBase.Email = member.User.Email;

      if (member.Id != owner.Id) {
        memberDtoBase.Links.Remove = MembersController.ResourceLinks.RemoveMember(member);
      }

      return memberDtoBase;
    }

    public string Role { get; internal set; }
    public string Username { get; internal set; }
    public string Email { get; internal set; }
    public dynamic Links { get; internal set; }
  }

  public class MemberExample : IExamplesProvider<MemberDto> {
    public MemberDto GetExamples() {
      var memberMock = new Member {
        Id = RandomNumberGenerator.GetInt32(1, 1000),
        CodeEventId = RandomNumberGenerator.GetInt32(1, 1000)
      };

      return new MemberDto {
        Email = "patrick@launchcode.org",
        Username = "the-vampiire",
        Role = MemberRole.Member.ToString(),
        Links = new { Remove = MembersController.ResourceLinks.RemoveMember(memberMock) },
      };
    }
  }

  public class MemberExamples : IExamplesProvider<List<MemberDto>> {
    public List<MemberDto> GetExamples() {
      var exampleGenerator = new MemberExample();

      var firstExample = exampleGenerator.GetExamples();

      var secondExample = exampleGenerator.GetExamples();
      secondExample.Email = "paul@launchcode.org";
      secondExample.Username = "pdmxdd";
      secondExample.Role = MemberRole.Owner.ToString();

      return new List<MemberDto> {
        firstExample,
        secondExample,
      };
    }
  }
}
