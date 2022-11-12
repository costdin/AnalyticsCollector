using Analytics.Database;
using AnalyticsCollector.DTOs;
using System.Linq;
using System.Net;

namespace AnalyticsCollector.Mappers
{
    public class AnalyticsEntryDtoMapper : IAnalyticsEntryDtoMapper
    {
        private readonly IBrowserResolver _browserResolver;

        public AnalyticsEntryDtoMapper(IBrowserResolver browserResolver)
        {
            _browserResolver = browserResolver;
        }

        public AddEventsDto ToDto(AnalyticsEntry entry)
        {
            return new AddEventsDto
            {
                SessionId = entry.SessionId,

                Events = entry.Events.Select(ToDto).ToArray()
            };
        }

        private AnalyticsEventDto ToDto(AnalyticsEvent analyticsEvent)
        {
            return new AnalyticsEventDto
            {
                ElementId = analyticsEvent.ElementId,
                EventTime = analyticsEvent.EventTime,
                EventType = analyticsEvent.EventType,
                X = analyticsEvent.X,
                Y = analyticsEvent.Y,
                Path = analyticsEvent.Path,
                Query = analyticsEvent.Query,
                Fragment = analyticsEvent.Fragment
            };
        }

        private AnalyticsEvent ToAnalyticsEvent(AnalyticsEventDto dto, IPAddress ip)
        {
            return new AnalyticsEvent
            {
                ClientIpAddress = ip,
                ElementId = dto.ElementId,
                ElementType = dto.ElementType,
                ElementClasses = dto.ElementClasses,
                ElementHref = dto.ElementHref,
                ElementText = dto.ElementText,
                EventTime = dto.EventTime,
                EventType = dto.EventType,
                X = dto.X,
                Y = dto.Y,
                Path = dto.Path,
                Query = dto.Query,
                Fragment = dto.Fragment
            };
        }

        public AnalyticsEntry ToAnalyticsEntry(AddEventsDto dto, string userAgent, IPAddress ip)
        {
            return new AnalyticsEntry
            {
                SessionId = dto.SessionId,

                Events = dto.Events
                    .Select(e => ToAnalyticsEvent(e, ip))
                    .ToArray()
            };
        }

        public AnalyticsEntry ToAnalyticsEntry(CreateSessionDto dto, string sessionId, string userAgent, IPAddress ipAddress)
        {
            return new AnalyticsEntry
            {
                SessionId = sessionId,
                SiteId = dto.SiteId,
                UserAgent = userAgent,
                Browser = _browserResolver.Resolve(userAgent),
                CustomProperties = dto.CustomProperties
            };
        }
    }
}
