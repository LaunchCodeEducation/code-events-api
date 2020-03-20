using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Http;

namespace CodeEventsAPI.Middleware {
  public class RegisterNewUserMiddleware {
    private readonly RequestDelegate _next;

    public RegisterNewUserMiddleware(RequestDelegate next) {
      _next = next;
    }

    public Task InvokeAsync(
      HttpContext context,
      CodeEventsDbContext dbContext
    ) {
      var authedUser = context.User;

      if (!authedUser.Identity.IsAuthenticated) {
        return _next(context);
      }

      if (authedUser.FindFirstValue("newUser") != "true") return _next(context);

      var user = new User(authedUser);

      var userExists =
        dbContext.Users.Count(u => u.AzureOId == user.AzureOId) == 1;
      if (userExists) return _next(context);

      dbContext.Users.Add(user);
      dbContext.SaveChanges();

      return _next(context);
    }
  }
}
