namespace AnalyticsCollector.Mappers
{
    public interface IBrowserResolver
    {
        string Resolve(string userAgent);
    }
}