using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CodeEventsAPI.Controllers;

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

  public class PublicCodeEventDto {
    public string Title { get; }
    public DateTime Date { get; }
    public dynamic Links { get; } // dynamic lets you edit the object on the fly

    internal PublicCodeEventDto(CodeEvent codeEvent) {
      Title = codeEvent.Title;
      Date = codeEvent.Date;
      Links = new {
        CodeEvent = CodeEventsController.Routes.GetCodeEvent(codeEvent),
        Join = CodeEventsController.Routes.JoinCodeEvent(codeEvent),
      };
    }
  }

  // do not serialize Members (recursive serialization exception)
  public class MemberCodeEventDto : PublicCodeEventDto {
    public string Description { get; }

    private MemberCodeEventDto(CodeEvent codeEvent)
      : base(codeEvent) {
      Description = codeEvent.Description;
      Links["Members"] = CodeEventsController.Routes.GetMembers(codeEvent);
    }

    public static MemberCodeEventDto ForMember(CodeEvent codeEvent) {
      var baseCodeEventDto = new MemberCodeEventDto(codeEvent);
      baseCodeEventDto.Links["Leave"] = CodeEventsController.Routes
        .LeaveCodeEvent(codeEvent);

      return baseCodeEventDto;
    }

    public static MemberCodeEventDto ForOwner(CodeEvent codeEvent) {
      var baseCodeEventDto = new MemberCodeEventDto(codeEvent);
      baseCodeEventDto.Links["Cancel"] = CodeEventsController.Routes
        .CancelCodeEvent(codeEvent);

      return baseCodeEventDto;
    }
  }

  // DTO to prevent over-posting and manage API validation
  public class NewCodeEventDto {
    [Required]
    [StringLength(
      40,
      MinimumLength = 10,
      ErrorMessage = "Title must be between 10 and 40 characters"
    )]
    public string Title { get; set; }

    [Required]
    [StringLength(
      1000,
      ErrorMessage = "Description can't be more than 1000 characters"
    )]
    public string Description { get; set; }

    [Required] public DateTime Date { get; set; }
  }
}
