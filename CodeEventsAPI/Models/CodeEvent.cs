using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CodeEventsAPI.Models {
  public class CodeEvent {
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }

    public virtual IEnumerable<Member> Members { get; set; }
  }

  /**
   * DTO to prevent over-posting
   */
  public class NewCodeEventDto {
    [Required]
    [StringLength(40, MinimumLength = 10,
      ErrorMessage = "Title must be between 10 and 40 characters")]
    public string Title { get; set; }

    [Required]
    [StringLength(1000,
      ErrorMessage = "Description can't be more than 1000 characters")]
    public string Description { get; set; }

    [Required] public DateTime Date { get; set; }
  }
}