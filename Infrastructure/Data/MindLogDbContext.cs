using Microsoft.EntityFrameworkCore;
using MindLog.Api.Core.Domain.Entities;

namespace MindLog.Api.Infrastructure.Data
{
    public class MindLogDbContext : DbContext
    {
        public MindLogDbContext(DbContextOptions<MindLogDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Emotion> Emotions { get; set; }
        public DbSet<ContextTag> ContextTags { get; set; }
        public DbSet<JournalEntry> JournalEntries { get; set; }
        public DbSet<EntryContext> EntryContexts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Emotion>().ToTable("emotions");
            modelBuilder.Entity<ContextTag>().ToTable("context_tags");
            modelBuilder.Entity<JournalEntry>().ToTable("journal_entries");
            modelBuilder.Entity<EntryContext>().ToTable("entry_contexts");

            modelBuilder.Entity<EntryContext>()
                .HasKey(ec => new { ec.EntryId, ec.TagId });

            modelBuilder.Entity<EntryContext>()
                .HasOne(ec => ec.JournalEntry)
                .WithMany(j => j.EntryContexts)
                .HasForeignKey(ec => ec.EntryId);

            modelBuilder.Entity<EntryContext>()
                .HasOne(ec => ec.ContextTag)
                .WithMany(t => t.EntryContexts)
                .HasForeignKey(ec => ec.TagId);

            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.User)
                .WithMany(u => u.JournalEntries)
                .HasForeignKey(j => j.UserId);
                
            modelBuilder.Entity<JournalEntry>()
                .HasOne(j => j.Emotion)
                .WithMany(e => e.JournalEntries)
                .HasForeignKey(j => j.EmotionId);
        }
    }
}