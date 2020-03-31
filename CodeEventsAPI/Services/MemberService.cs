using System.Security.Claims;
using System.Linq;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;

namespace CodeEventsAPI.Services {
  public interface IMemberService {
    bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser);

    bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser);

    bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser);

    bool DoesMemberExist(long memberId);
  }

  public class MemberService : IMemberService {
    private readonly CodeEventsDbContext _dbContext;
    private readonly IUserTransferenceService _userTransferenceService;

    public MemberService(
      CodeEventsDbContext dbContext,
      IUserTransferenceService userTransferenceService
    ) {
      _dbContext = dbContext;
      _userTransferenceService = userTransferenceService;
    }

    public bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser) {
      var isMember = IsUserAMember(codeEventId, authedUser);

      return !isMember;
    }

    public bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser) {
      var codeEventCount = _dbContext.CodeEvents.Count(ce => ce.Id == codeEventId);
      var codeEventExists = codeEventCount == 1;
      if (!codeEventExists) return false;

      var user = _userTransferenceService.ConvertAuthedUserToUser(authedUser);
      var memberCount = _dbContext.Members.Count(
        m => m.UserId == user.Id && m.CodeEventId == codeEventId
      );
      var isMember = memberCount == 1;

      return isMember;
    }

    public bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser) {
      var isMember = IsUserAMember(codeEventId, authedUser);
      if (!isMember) return false;

      var requestingMember =
        _userTransferenceService.ConvertAuthedUserToMember(codeEventId, authedUser);

      return requestingMember.Role == MemberRole.Owner;
    }

    public bool DoesMemberExist(long memberId) {
      var memberCount = _dbContext.Members.Count(m => m.Id == memberId);
      var memberExists = memberCount == 1;

      return memberExists;
    }
  }
}
