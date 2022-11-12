namespace AnalyticsCollector.Mappers
{
    public class BrowserResolver : IBrowserResolver
    {
        public string Resolve(string userAgent)
        {
            if (userAgent.Contains("Chrome"))
            {
                return "Chrome";
            }
            else if (userAgent.Contains("MSIE"))
            {
                return "Internet Explorer";
            }
            else if (userAgent.Contains("Firefox"))
            {
                return "Firefox";
            }
            else
            {
                return "Other";
            }
        }
    }
}