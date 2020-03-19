using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.AspNetCore.Http;

namespace CodeEventsAPI.Middleware {
  public class AddUserIdClaimMiddleware {
    private readonly RequestDelegate _next;

    public AddUserIdClaimMiddleware(RequestDelegate next) {
      _next = next;
    }

    public async Task InvokeAsync(HttpContext context,
      CodeEventsDbContext dbContext) {
      var authedUser = context.User;
      if (!authedUser.Identity.IsAuthenticated || authedUser.FindFirstValue("userId") != null) {
        await _next(context);
        return;
      }

      var user = new User(authedUser);
      long userId = dbContext.Users
        .First(u => u.AzureOId == user.AzureOId)
        .Id;

      // inject user id into context.User
      context.User.Identities.FirstOrDefault()
        ? // prevent NPE if no identity is found (unlikely but suppress warning)
        .AddClaim(new Claim("userId", userId.ToString()));

      await _next(context);
    }
  }
}