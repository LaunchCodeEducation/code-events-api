using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CodeEventsAPI.Controllers;
using CodeEventsAPI.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CodeEventsAPI.Swagger {
  public class NewCodeEventExample : IExamplesProvider<NewCodeEventDto> {
    public NewCodeEventDto GetExamples() {
      return new NewCodeEventDto {
        Title = "New Code Event title",
        Description = "New Code Event description",
        Date = DateTime.Now,
      };
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
