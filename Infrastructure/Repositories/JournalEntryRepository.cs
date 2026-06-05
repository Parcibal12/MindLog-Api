using Microsoft.EntityFrameworkCore;
using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Data;

namespace MindLog.Api.Infrastructure.Repositories
{
    public class JournalEntryRepository : IJournalEntryRepository
    {
        private readonly MindLogDbContext _context;

        public JournalEntryRepository(MindLogDbContext context)
        {
            _context = context;
        }

        public async Task<JournalEntry?> GetByIdAsync(Guid id)
        {
            return await _context.JournalEntries
                .Include(j => j.Emotion)
                .Include(j => j.EntryContexts)
                    .ThenInclude(ec => ec.ContextTag)
                .FirstOrDefaultAsync(j => j.Id == id && j.DeletedAt == null);
        }

        public async Task<IEnumerable<JournalEntry>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.JournalEntries
                .Include(j => j.Emotion)
                .Include(j => j.EntryContexts)
                    .ThenInclude(ec => ec.ContextTag)
                .Where(j => j.UserId == userId && j.DeletedAt == null)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetByUserIdAndDateRangeAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Emotion)
                .Where(j => j.UserId == userId && j.CreatedAt >= startDate && j.CreatedAt <= endDate && j.DeletedAt == null)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JournalEntry> AddAsync(JournalEntry entry)
        {
            await _context.JournalEntries.AddAsync(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        public async Task UpdateAsync(JournalEntry entry)
        {
            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry != null)
            {
                entry.DeletedAt = DateTime.UtcNow;
                _context.JournalEntries.Update(entry);
                await _context.SaveChangesAsync();
            }
        }
    }
}