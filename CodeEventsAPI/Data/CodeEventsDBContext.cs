using CodeEventsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

namespace CodeEventsAPI.Data {
  public class CodeEventsDbContext : DbContext {
    public CodeEventsDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Member>().HasKey(member => member.Id);

      // TODO: store enum values as strings?
      // modelBuilder.Entity<Member>().Property(m => m.Role).HasConversion();

      modelBuilder.Entity<Member>()
        .HasIndex(member => new {
          member.UserId, member.CodeEventId
        })
        .IsUnique();

      modelBuilder.Entity<Member>()
        .HasOne(member => member.User)
        .WithMany(user => user.Memberships)
        .HasForeignKey(member => member.UserId);

      modelBuilder.Entity<Member>()
        .HasOne(member => member.CodeEvent)
        .WithMany(codeEvent => codeEvent.Members)
        .HasForeignKey(member => member.CodeEventId);
    }

    public DbSet<CodeEvent> Events { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<Member> Members { get; set; }
  }
}