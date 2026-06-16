using Microsoft.EntityFrameworkCore;
using MindLog.Api.Core.Domain.Entities;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Data;
using MindLog.Api.Core.Application.Services.DTOs;

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
                .AsNoTracking()
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
                .AsNoTracking()
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

        public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var query = _context.JournalEntries
                .AsNoTracking() 
                .Where(j => j.UserId == userId && j.DeletedAt == null && j.CreatedAt >= startDate && j.CreatedAt <= endDate);

            var totalEntries = await query.CountAsync();

            if (totalEntries == 0) return new AnalyticsSummaryDto(); 

            var dominantEmotion = await query
                .Where(j => j.Emotion != null)
                .GroupBy(j => j.Emotion!.Name)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync() ?? "Ninguna";

            var dominantPattern = await query
                .Where(j => j.AiPattern != null)
                .GroupBy(j => j.AiPattern)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync() ?? "Ninguno";

            var emotionDist = await query
                .Where(j => j.Emotion != null)
                .GroupBy(j => j.Emotion!.Name)
                .Select(g => new { Emotion = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Emotion, x => x.Count);

            var moodTrend = await query
                .GroupBy(j => j.CreatedAt.Date)
                .Select(g => new DailyIntensityDto
                {
                    Date = g.Key,
                    AverageIntensity = g.Average(x => x.Intensity)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var topTags = await _context.JournalEntries
                .AsNoTracking()
                .Where(j => j.UserId == userId && j.DeletedAt == null && j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                .SelectMany(j => j.EntryContexts)
                .Where(ec => ec.ContextTag != null)
                .GroupBy(ec => ec.ContextTag!.Name)
                .Select(g => new ContextTagCountDto
                {
                    TagName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            if (topTags.Any())
            {
                var tagNames = topTags.Select(t => t.TagName).ToList();

                var tagEmotions = await _context.JournalEntries
                    .AsNoTracking()
                    .Where(j => j.UserId == userId && j.DeletedAt == null && j.CreatedAt >= startDate && j.CreatedAt <= endDate)
                    .SelectMany(j => j.EntryContexts)
                    .Where(ec => ec.ContextTag != null && tagNames.Contains(ec.ContextTag!.Name))
                    .Select(ec => new { 
                        Tag = ec.ContextTag!.Name, 
                        Emotion = ec.JournalEntry != null && ec.JournalEntry.Emotion != null ? ec.JournalEntry.Emotion.Name : "Ninguna" 
                    })
                    .ToListAsync();

                foreach (var tag in topTags)
                {
                    tag.DominantEmotion = tagEmotions
                        .Where(te => te.Tag == tag.TagName)
                        .GroupBy(te => te.Emotion)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault() ?? dominantEmotion;
                }
            }

            return new AnalyticsSummaryDto
            {
                TotalEntries = totalEntries,
                DominantEmotion = dominantEmotion,
                DominantPattern = dominantPattern,
                EmotionDistribution = emotionDist,
                MoodTrend = moodTrend,
                TopDisparadores = topTags
            };
        }

        public async Task<List<DateTime>> GetEntryDatesAsync(Guid userId)
        {
            return await _context.JournalEntries
                .Where(j => j.UserId == userId && j.DeletedAt == null)
                .Select(j => j.CreatedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
        }
    }
}