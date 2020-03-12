using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace CodeEventsAPI.Data {
  public class CodeEventsDBContext : DbContext {
    public CodeEventsDBContext(DbContextOptions options) : base(options) { }

    public DbSet<CodeEvent> Events { get; set; }
    // TODO: User set, EventMember? set
  }
}