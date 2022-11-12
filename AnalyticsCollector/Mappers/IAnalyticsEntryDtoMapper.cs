using Analytics.Database;
using AnalyticsCollector.DTOs;
using System.Net;

namespace AnalyticsCollector.Mappers
{
    public interface IAnalyticsEntryDtoMapper
    {
        AnalyticsEntry ToAnalyticsEntry(AddEventsDto dto, string userAgent, IPAddress ip);
        AnalyticsEntry ToAnalyticsEntry(CreateSessionDto dto, string sessionId, string userAgent, IPAddress ipAddress);

        AddEventsDto ToDto(AnalyticsEntry entry);
    }
}