using MindLog.Api.Core.Application.Services.DTOs;
using MindLog.Api.Core.Domain.Interfaces;
using MindLog.Api.Infrastructure.Data;

namespace MindLog.Api.Core.Application.Services
{
    public class UserService : IUserService
    {
        private readonly MindLogDbContext _context;

        public UserService(MindLogDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto request)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null) 
                return false;

            user.TherapistName = request.TherapistName;
            user.TherapistEmail = request.TherapistEmail;
            user.AutoSendReports = request.AutoSendReports;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}