using Analytics.Database.ConnectionFactories;
using Analytics.Database.QueryBuilders;
using System.Threading.Tasks;

namespace Analytics.Database
{
    public class FunnelRepository : IFunnelRepository
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IFunnelQueryBuilder _funnelQueryBuilder;

        public FunnelRepository(
            IConnectionFactory connectionFactory,
            IFunnelQueryBuilder funnelQueryBuilder)
        {
            _connectionFactory = connectionFactory;
            _funnelQueryBuilder = funnelQueryBuilder;
        }

        public async Task<int[]> FunnelAnalysis(
            Funnel[] funnels,
            params string[] indexList)
        {
            var client = _connectionFactory.GetClient();

            var queryBuilder = _funnelQueryBuilder
                .Build()
                .WithIndexes(indexList)
                .WithFunnels(funnels);

            var query = queryBuilder.CreateQuery();

            var response = await client.SearchAsync(query);

            if (!response.IsValid)
            {
                throw response.OriginalException;
            }

            var resultExtractor = queryBuilder.CreateResultExtractor();

            return resultExtractor(response);
        }

    }
}
