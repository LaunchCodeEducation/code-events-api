using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace CodeEventsAPI.Data {
  public class CodeEventsDbContext : DbContext {
    public CodeEventsDbContext(DbContextOptions options) : base(options) { }

    public DbSet<CodeEvent> Events { get; set; }

    public DbSet<User> Users { get; set; }
    // TODO: User set, EventMember? set
  }
}