using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeEventsAPI {
  public interface ICodeEventService {
    List<CodeEventDto> GetAllCodeEvents();

    CodeEventDto GetCodeEventById(long codeEventId);

    CodeEventDto RegisterCodeEvent(
      NewCodeEventDto newCodeEventDto,
      ClaimsPrincipal authedUser
    );

    List<MemberDto> GetAllMembers(long codeEventId);

    bool CanUserRegisterAsMember(long codeEventId, ClaimsPrincipal authedUser);

    void RegisterMember(long codeEventId, ClaimsPrincipal authedUser);
  }

  public class CodeEventService : ICodeEventService {
    private readonly CodeEventsDbContext _dbContext;

    public CodeEventService(CodeEventsDbContext dbContext) {
      _dbContext = dbContext;
    }

    private User ConvertAuthedUser(ClaimsPrincipal authedUser) {
      var authedUserId = Convert.ToInt64(authedUser.FindFirstValue("userId"));
      return _dbContext.Users.Find(authedUserId);
    }

    public CodeEventDto GetCodeEventById(long codeEventId) {
      return _dbContext.CodeEvents.Find(codeEventId)?.ToDto();
    }

    public List<CodeEventDto> GetAllCodeEvents() {
      return _dbContext.CodeEvents.Select(ce => ce.ToDto()).ToList();
    }

    public CodeEventDto RegisterCodeEvent(
      NewCodeEventDto newCodeEvent,
      ClaimsPrincipal authedUser
    ) {
      var owner = ConvertAuthedUser(authedUser);

      var codeEventEntry = _dbContext.CodeEvents.Add(new CodeEvent());
      codeEventEntry.CurrentValues.SetValues(newCodeEvent);
      var codeEvent = codeEventEntry.Entity;

      _dbContext.Members.Add(Member.CreateEventOwner(codeEvent, owner));

      _dbContext.SaveChanges();

      return codeEvent.ToDto();
    }

    public List<MemberDto> GetAllMembers(long codeEventId) {
      var codeEvent = _dbContext.CodeEvents.Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .SingleOrDefault(ce => ce.Id == codeEventId);

      return codeEvent?.Members.Select(member => member.ToDto()).ToList();
    }

    /**
     * guard clauses:
     * - CodeEvent not found -> false
     * - already a Member -> false
     */
    public bool CanUserRegisterAsMember(
      long codeEventId,
      ClaimsPrincipal authedUser
    ) {
      var codeEventExists =
        _dbContext.CodeEvents.Count(ce => ce.Id == codeEventId) == 1;
      if (!codeEventExists) return false;

      var user = ConvertAuthedUser(authedUser);
      var memberCount = _dbContext.Members.Count(
        m => m.UserId == user.Id && m.CodeEventId == codeEventId
      );

      return memberCount == 0;
    }

    public void RegisterMember(long codeEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUser(authedUser);

      _dbContext.Members.Add(Member.CreateEventMember(codeEventId, user.Id));
      _dbContext.SaveChanges();
    }
  }
}
