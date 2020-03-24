using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public class CodeEvent {
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public List<Member> Members { get; set; }

    public CodeEventDto ToDto() {
      return new CodeEventDto(this);
    }
  }

  // TODO: implement, replace CodeEventDto?
  /*
  EventFullDTO (role: member, owner)
    all: id
    all: title
    all: date
    all: description
    all: owner
    links:
      members: @member, @owner
      leave: @member
      cancel: @owner
  */

  // do not serialize Members (recursive serialization exception)
  public class CodeEventDto {
    public long Id { get; }
    public string Title { get; }
    public string Description { get; }
    public DateTime Date { get; }

    public CodeEventDto(CodeEvent codeEvent) {
      Id = codeEvent.Id;
      Title = codeEvent.Title;
      Description = codeEvent.Description;
      Date = codeEvent.Date;
    }

    // TODO: HAL links prop?
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
