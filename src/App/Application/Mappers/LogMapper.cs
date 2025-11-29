using App.Application.DTOs;
using Common.Infrastructure.Data;

namespace App.Application.Mappers;

public static class LogMapper
{
    public static LogDTO GetDTO(Log log)
    {
        return new LogDTO
        {
            Id = log.Id,
            UserId = log.UserId,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Details = log.Details,
            CreatedAt = log.CreatedAt
        };
    }
}
