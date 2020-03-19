using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CodeEventsAPI.Data;
using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeEventsAPI {
  public interface ICodeEventService {
    List<CodeEvent> GetAllCodeEvents();

    CodeEvent GetCodeEventById(long codeEventId);

    CodeEvent RegisterCodeEvent(
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
      var user = new User(authedUser);
      user.Id = Convert.ToInt64(authedUser.FindFirstValue("userId"));

      /**
       * System.InvalidOperationException: The instance of entity type 'User' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
       */

      return user;
    }

    public CodeEvent GetCodeEventById(long codeEventId) {
      return _dbContext.CodeEvents.Find(codeEventId);
    }

    public List<CodeEvent> GetAllCodeEvents() {
      return _dbContext.CodeEvents.ToList();
    }

    public CodeEvent RegisterCodeEvent(
      NewCodeEventDto newCodeEvent,
      ClaimsPrincipal authedUser
    ) {
      var owner = ConvertAuthedUser(authedUser);

      var codeEventEntry = _dbContext.CodeEvents.Add(new CodeEvent());
      codeEventEntry.CurrentValues.SetValues(newCodeEvent);
      var codeEvent = codeEventEntry.Entity;

      _dbContext.Members.Add(Member.CreateEventOwner(codeEvent, owner));

      _dbContext.SaveChanges();

      return codeEvent;
    }

    public List<MemberDto> GetAllMembers(long codeEventId) {
      var codeEvent = _dbContext.CodeEvents.Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .Single(ce => ce.Id == codeEventId);

      return codeEvent.Members.Select(member => member.ToDto()).ToList();
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
      var codeEventExists = _dbContext.CodeEvents.Count(ce =>
        ce.Id == codeEventId) == 1;
      if (!codeEventExists) return false;

      var user = ConvertAuthedUser(authedUser);
      var memberCount = _dbContext.Members.Count(m =>
        m.UserId == user.Id && m.CodeEventId == codeEventId);

      return memberCount == 0;
    }

    public void RegisterMember(long codeEventId, ClaimsPrincipal authedUser) {
      var user = ConvertAuthedUser(authedUser);

      _dbContext.Members.Add(Member.CreateEventMember(codeEventId, user.Id));
    }
  }
}
