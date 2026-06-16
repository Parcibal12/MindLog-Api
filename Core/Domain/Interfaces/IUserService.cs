using MindLog.Api.Core.Application.Services.DTOs;

namespace MindLog.Api.Core.Domain.Interfaces
{
    public interface IUserService
    {
        Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto request);
    }
}