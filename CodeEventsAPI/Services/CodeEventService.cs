using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeEventsAPI.Services {
  public interface ICodeEventService {
    List<PublicCodeEventDto> GetAllCodeEvents();

    MemberCodeEventDto GetCodeEventById(
      long codeEventId,
      ClaimsPrincipal authedUser
    );

    CodeEvent RegisterCodeEvent(
      NewCodeEventDto newCodeEventDto,
      ClaimsPrincipal authedUser
    );

    bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser);

    bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser);

    bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser);

    bool DoesMemberExist(long memberId);

    List<MemberDto> GetMembersList(
      long codeEventId,
      ClaimsPrincipal authedUser
    );

    void JoinCodeEvent(long codeEventId, ClaimsPrincipal authedUser);

    void RemoveMember(long memberId);

    void RemoveMember(Member memberToRemove);

    void LeaveCodeEvent(long codeEventId, ClaimsPrincipal authedUser);

    void CancelCodeEvent(long codeEventId);
  }

  public class CodeEventService : ICodeEventService {
    private readonly CodeEventsDbContext _dbContext;

    public CodeEventService(CodeEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    private User ConvertAuthedUserToUser(ClaimsPrincipal authedUser) {
      var authedUserId = Convert.ToInt64(authedUser.FindFirstValue("userId"));
      return _dbContext.Users.Find(authedUserId);
    }

    private Member ConvertAuthedUserToMember(
      long codeEventId,
      ClaimsPrincipal authedUser
    ) {
      var user = ConvertAuthedUserToUser(authedUser);

      return _dbContext.Members.First(
        m => m.UserId == user.Id && m.CodeEventId == codeEventId
      );
    }

    public MemberCodeEventDto GetCodeEventById(
      long codeEventId,
      ClaimsPrincipal authedUser
    ) {
      var requestingMember = ConvertAuthedUserToMember(codeEventId, authedUser);
      return _dbContext.CodeEvents.Find(codeEventId)
        ?.ToMemberDto(requestingMember);
    }

    public List<PublicCodeEventDto> GetAllCodeEvents() {
      return _dbContext.CodeEvents.Select(ce => ce.ToPublicDto()).ToList();
    }

    public CodeEvent RegisterCodeEvent(
      NewCodeEventDto newCodeEvent,
      ClaimsPrincipal authedUser
    ) {
      var user = ConvertAuthedUserToUser(authedUser);

      var codeEventEntry = _dbContext.CodeEvents.Add(new CodeEvent());
      codeEventEntry.CurrentValues.SetValues(newCodeEvent);
      var codeEvent = codeEventEntry.Entity;

      _dbContext.Members.Add(Member.CreateEventOwner(codeEvent, user));

      _dbContext.SaveChanges();

      return codeEvent;
    }

    public List<MemberDto> GetMembersList(
      long codeEventId,
      ClaimsPrincipal authedUser
    ) {
      var requestingMember = ConvertAuthedUserToMember(codeEventId, authedUser);

      var codeEvent = _dbContext.CodeEvents.Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .SingleOrDefault(ce => ce.Id == codeEventId);


      return codeEvent?.Members.Select(member => member.ToDto(requestingMember))
        .ToList();
    }

    public bool CanUserRegisterAsMember(
      long codeEventId,
      ClaimsPrincipal authedUser
    ) {
      var isMember = IsUserAMember(codeEventId, authedUser);

      return !isMember;
    }

    public bool IsUserAMember(long codeEventId, ClaimsPrincipal authedUser) {
      var codeEventCount =
        _dbContext.CodeEvents.Count(ce => ce.Id == codeEventId);
      var codeEventExists = codeEventCount == 1;
      if (!codeEventExists) return false;

      var user = ConvertAuthedUserToUser(authedUser);
      var memberCount = _dbContext.Members.Count(
        m => m.UserId == user.Id && m.CodeEventId == codeEventId
      );
      var isMember = memberCount == 1;

      return isMember;
    }

    public bool IsUserAnOwner(long codeEventId, ClaimsPrincipal authedUser) {
      var isMember = IsUserAMember(codeEventId, authedUser);
      if (!isMember) return false;

      var requestingMember = ConvertAuthedUserToMember(codeEventId, authedUser);

      return requestingMember.Role == MemberRole.Owner;
    }

    public bool DoesMemberExist(long memberId) {
      var memberCount = _dbContext.Members.Count(m => m.Id == memberId);
      var memberExists = memberCount == 1;

      return memberExists;
    }

    public void JoinCodeEvent(long codeEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUserToUser(authedUser);

      _dbContext.Members.Add(Member.CreateEventMember(codeEventId, user.Id));
      _dbContext.SaveChanges();
    }

    public void RemoveMember(Member memberToRemove) {
      _dbContext.Members.Remove(memberToRemove);
      _dbContext.SaveChanges();
    }

    public void RemoveMember(long memberId) {
      var memberProxy = new Member() { Id = memberId };
      RemoveMember(memberProxy);
    }

    public void LeaveCodeEvent(long codeEventId, ClaimsPrincipal authedUser) {
      var leavingMember = ConvertAuthedUserToMember(codeEventId, authedUser);
      RemoveMember(leavingMember);
    }

    public void CancelCodeEvent(long codeEventId) {
      var codeEventProxy = new CodeEvent() { Id = codeEventId };
      _dbContext.CodeEvents.Remove(codeEventProxy);
      _dbContext.SaveChanges();
    }
  }
}
