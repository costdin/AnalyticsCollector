using Nest;

namespace Analytics.Database.ConnectionFactories
{
    public interface IConnectionFactory
    {
        ElasticClient GetClient();
    }
}
