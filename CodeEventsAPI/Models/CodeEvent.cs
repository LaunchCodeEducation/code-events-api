using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Security.Cryptography;
using CodeEventsAPI.Controllers;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace CodeEventsAPI.Models {
  public class CodeEvent {
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public List<Member> Members { get; set; }

    public PublicCodeEventDto ToPublicDto() {
      return new PublicCodeEventDto(this);
    }

    public MemberCodeEventDto ToMemberDto(Member requestingMember) {
      return requestingMember.Role switch {
        MemberRole.Owner => MemberCodeEventDto.ForOwner(this),
        MemberRole.Member => MemberCodeEventDto.ForMember(this),
        _ => null,
      };
    }
  }


  // DTO to prevent over-posting and manage API validation
  public class NewCodeEventDto {
    [NotNull]
    [Required]
    [StringLength(
      40,
      MinimumLength = 10,
      ErrorMessage = "Title must be between 10 and 40 characters"
    )]
    public string Title { get; set; }

    [NotNull]
    [Required]
    [StringLength(1000, ErrorMessage = "Description can't be more than 1000 characters")]
    public string Description { get; set; }

    [Required] [NotNull] public DateTime Date { get; set; }
  }

  public class NewCodeEventExample : IExamplesProvider<NewCodeEventDto> {
    public NewCodeEventDto GetExamples() {
      return new NewCodeEventDto {
        Title = "New Code Event title",
        Description = "New Code Event description",
        Date = DateTime.Now,
      };
    }
  }

  public abstract class BaseCodeEventDto {
    public string Title { get; internal set; }
    public DateTime Date { get; internal set; }

    public dynamic Links { get; internal set; } // dynamic lets you edit the object on the fly

    protected BaseCodeEventDto() { }

    internal BaseCodeEventDto(CodeEvent codeEvent) {
      Title = codeEvent.Title;
      Date = codeEvent.Date;
      Links = new ExpandoObject();

      Links.CodeEvent = CodeEventsController.ResourceLinks.GetCodeEvent(codeEvent);
    }
  }

  public class PublicCodeEventDto : BaseCodeEventDto {
    public PublicCodeEventDto() { }

    public PublicCodeEventDto(CodeEvent codeEvent)
      : base(codeEvent) {
      Links.Join = MembersController.ResourceLinks.JoinCodeEvent(codeEvent);
    }
  }

  public class PublicCodeEventExample : IExamplesProvider<PublicCodeEventDto> {
    public PublicCodeEventDto GetExamples() {
      var mockCodeEvent = new CodeEvent { Id = RandomNumberGenerator.GetInt32(1, 1000) };

      return new PublicCodeEventDto {
        Title = "LaunchCode: Introduction to Azure",
        Date = DateTime.Today,
        Links = new {
          CodeEvent = CodeEventsController.ResourceLinks.GetCodeEvent(mockCodeEvent),
          Join = MembersController.ResourceLinks.JoinCodeEvent(mockCodeEvent)
        }
      };
    }
  }

  public class PublicCodeEventExamples : IExamplesProvider<List<PublicCodeEventDto>> {
    public List<PublicCodeEventDto> GetExamples() {
      var exampleGenerator = new PublicCodeEventExample();

      var firstExample = exampleGenerator.GetExamples();

      var secondExample = exampleGenerator.GetExamples();
      secondExample.Title = "LaunchCode: Introduction to DS & A";

      return new List<PublicCodeEventDto> {
        firstExample,
        secondExample,
      };
    }
  }

  public class MemberCodeEventDto : BaseCodeEventDto {
    public string Description { get; internal set; }

    public MemberCodeEventDto() { }

    private MemberCodeEventDto(CodeEvent codeEvent)
      : base(codeEvent) {
      Description = codeEvent.Description;
      Links.Members = MembersController.ResourceLinks.GetMembers(codeEvent);
    }

    public static MemberCodeEventDto ForMember(CodeEvent codeEvent) {
      var baseCodeEventDto = new MemberCodeEventDto(codeEvent);
      baseCodeEventDto.Links.Leave = MembersController.ResourceLinks.LeaveCodeEvent(codeEvent);

      return baseCodeEventDto;
    }

    public static MemberCodeEventDto ForOwner(CodeEvent codeEvent) {
      var baseCodeEventDto = new MemberCodeEventDto(codeEvent);
      baseCodeEventDto.Links.Cancel = CodeEventsController.ResourceLinks.CancelCodeEvent(codeEvent);

      return baseCodeEventDto;
    }
  }

  public class MemberCodeEventExample : IExamplesProvider<MemberCodeEventDto> {
    public MemberCodeEventDto GetExamples() {
      var mockCodeEvent = new CodeEvent { Id = RandomNumberGenerator.GetInt32(1, 1000) };

      return new MemberCodeEventDto {
        Title = "LaunchCode: Introduction to Azure",
        Date = DateTime.Today,
        Description = "A one week course on deploying a RESTful API to Azure",
        Links = new {
          CodeEvent = CodeEventsController.ResourceLinks.GetCodeEvent(mockCodeEvent),
          Leave = MembersController.ResourceLinks.LeaveCodeEvent(mockCodeEvent),
          Cancel = CodeEventsController.ResourceLinks.CancelCodeEvent(mockCodeEvent)
        }
      };
    }
  }

  public class MemberCodeEventExamples : IExamplesProvider<List<MemberCodeEventDto>> {
    public List<MemberCodeEventDto> GetExamples() {
      var exampleGenerator = new MemberCodeEventExample();

      var firstExample = exampleGenerator.GetExamples();

      var secondExample = exampleGenerator.GetExamples();
      secondExample.Title = "LaunchCode: Introduction to DS & A";
      secondExample.Description = "A one week course on Data Structures & Algorithms";

      return new List<MemberCodeEventDto> {
        firstExample,
        secondExample,
      };
    }
  }
}
