using System.Security.Claims;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CodeEventsAPI.Middleware {
  public class RegisterNewUserMiddleware {
    private readonly RequestDelegate _next;

    public RegisterNewUserMiddleware(RequestDelegate next) {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
      CodeEventsDbContext dbContext) {
      var authedUser = context.User;

      if (!authedUser.Identity.IsAuthenticated) {
        await _next(context);
        return;
      }

      if (authedUser.FindFirstValue("newUser") == "true") {
        dbContext.Users.Add(new User(authedUser));
        try {
          await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException e) {
          // users token still has "newUser" flag but has already been registered
          // TODO: more elegant way of handling this? how to clear the flag?
        }
      }

      await _next(context);
    }
  }
}