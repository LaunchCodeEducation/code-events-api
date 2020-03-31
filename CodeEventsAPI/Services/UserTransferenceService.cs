using System;
using System.Linq;
using System.Security.Claims;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;

namespace CodeEventsAPI.Services {
  public interface IUserTransferenceService {
    User GetOrCreateUserFromActiveDirectory(ClaimsPrincipal activeDirectoryUser);

    User ConvertAuthedUserToUser(ClaimsPrincipal authedUser);

    Member ConvertAuthedUserToMember(long codeEventId, ClaimsPrincipal authedUser);
  }

  public class UserTransferenceService : IUserTransferenceService {
    private readonly CodeEventsDbContext _dbContext;

    public UserTransferenceService(CodeEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    public User GetOrCreateUserFromActiveDirectory(ClaimsPrincipal activeDirectoryUser) {
      var newUser = new User(activeDirectoryUser);

      var existingUser =
        _dbContext.Users.FirstOrDefault(u => u.AzureOId == newUser.AzureOId);
      if (existingUser != null) return existingUser;

      _dbContext.Users.Add(newUser);
      _dbContext.SaveChanges();
      return newUser;
    }

    public User ConvertAuthedUserToUser(ClaimsPrincipal authedUser) {
      var authedUserId = Convert.ToInt64(authedUser.FindFirstValue("userId"));
      return _dbContext.Users.Find(authedUserId);
    }

    public Member ConvertAuthedUserToMember(long codeEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUserToUser(authedUser);

      return _dbContext.Members.First(m => m.UserId == user.Id && m.CodeEventId == codeEventId);
    }
  }
}
