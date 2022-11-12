using System.Threading.Tasks;

namespace Analytics.Database
{
    public interface IAnalyticsRepository
    {
        Task SaveSession(AnalyticsEntry entry);

        Task SaveEvents(string sessionId, AnalyticsEvent[] events);
    }
}