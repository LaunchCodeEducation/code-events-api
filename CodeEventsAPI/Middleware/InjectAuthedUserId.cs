using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CodeEventsAPI.Middleware {
  public class InjectAuthedUserIdMiddleware {
    private readonly RequestDelegate _next;

    public InjectAuthedUserIdMiddleware(RequestDelegate next) {
      _next = next;
    }

// TODO: rename to "RegisterNewUserMiddleware"
// look into why context.User.Claims can not be mutated
// for downstream convenience of having userId
    public async Task InvokeAsync(HttpContext context,
      CodeEventsDbContext dbContext) {
      var authedUser = context.User;
      if (authedUser.FindFirstValue("userId") != null) {
        await _next(context);
        return;
      }

      // long userId;
      var user = new User(authedUser);

      if (context.User.FindFirstValue("newUser") == "true") {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        // userId = user.Id; // available after saving
      }

      // else {
      //   // user ID from existing user
      //   userId = dbContext.Users
      //     .First(u => u.AzureOId == user.AzureOId)
      //     .Id;
      // }
      //
      // // inject user id into context.User
      // authedUser.Claims.Append(new Claim("userId", userId.ToString()));
      await _next(context);
    }
  }
}