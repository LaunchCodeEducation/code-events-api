using System.Collections.Generic;
using System.Security.Cryptography;
using CodeEventsAPI.Controllers;
using CodeEventsAPI.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodeEventsAPI.Swagger {
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
