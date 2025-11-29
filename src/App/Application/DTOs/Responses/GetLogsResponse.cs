using Common.Abstracts;
using Common.DTOs;
using App.Application.DTOs;

namespace App.Application.DTOs.Responses;

public class GetLogsResponse(PagedResponse<LogDTO> logs) : IResponse
{
    public PagedResponse<LogDTO> Logs { get; set; } = logs;
}