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

    MemberCodeEventDto GetCodeEventById(long codeEventId, ClaimsPrincipal authedUser);

    CodeEvent RegisterCodeEvent(NewCodeEventDto newCodeEventDto, ClaimsPrincipal authedUser);

    List<MemberDto> GetMembersList(long codeEventId, ClaimsPrincipal authedUser);

    void JoinCodeEvent(long codeEventId, ClaimsPrincipal authedUser);

    void RemoveMember(long memberId);

    void RemoveMember(Member memberToRemove);

    void LeaveCodeEvent(long codeEventId, ClaimsPrincipal authedUser);

    void CancelCodeEvent(long codeEventId);
  }

  public class CodeEventService : ICodeEventService {
    private readonly CodeEventsDbContext _dbContext;
    private readonly IUserTransferenceService _userTransferenceService;

    public CodeEventService(
      CodeEventsDbContext dbContext,
      IUserTransferenceService userTransferenceService
    ) {
      _dbContext = dbContext;
      _userTransferenceService = userTransferenceService;
    }

    public MemberCodeEventDto GetCodeEventById(long codeEventId, ClaimsPrincipal authedUser) {
      var requestingMember =
        _userTransferenceService.ConvertAuthedUserToMember(codeEventId, authedUser);
      return _dbContext.CodeEvents.Find(codeEventId)?.ToMemberDto(requestingMember);
    }

    public List<PublicCodeEventDto> GetAllCodeEvents() {
      return _dbContext.CodeEvents.Select(ce => ce.ToPublicDto()).ToList();
    }

    public CodeEvent RegisterCodeEvent(NewCodeEventDto newCodeEvent, ClaimsPrincipal authedUser) {
      var user = _userTransferenceService.ConvertAuthedUserToUser(authedUser);

      var codeEventEntry = _dbContext.CodeEvents.Add(new CodeEvent());
      codeEventEntry.CurrentValues.SetValues(newCodeEvent);
      var codeEvent = codeEventEntry.Entity;

      _dbContext.Members.Add(Member.CreateEventOwner(codeEvent, user));

      _dbContext.SaveChanges();

      return codeEvent;
    }

    public List<MemberDto> GetMembersList(long codeEventId, ClaimsPrincipal authedUser) {
      var requestingMember =
        _userTransferenceService.ConvertAuthedUserToMember(codeEventId, authedUser);

      var codeEvent = _dbContext.CodeEvents.Include(ce => ce.Members)
        .ThenInclude(m => m.User)
        .SingleOrDefault(ce => ce.Id == codeEventId);


      return codeEvent?.Members.Select(member => member.ToDto(requestingMember)).ToList();
    }


    public void JoinCodeEvent(long codeEventId, ClaimsPrincipal authedUser) {
      var user = _userTransferenceService.ConvertAuthedUserToUser(authedUser);

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
      var leavingMember =
        _userTransferenceService.ConvertAuthedUserToMember(codeEventId, authedUser);
      RemoveMember(leavingMember);
    }

    public void CancelCodeEvent(long codeEventId) {
      var codeEventProxy = new CodeEvent() { Id = codeEventId };
      _dbContext.CodeEvents.Remove(codeEventProxy);
      _dbContext.SaveChanges();
    }
  }
}
