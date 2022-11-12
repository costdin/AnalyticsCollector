using Nest;
using System;
using System.Collections.Generic;

namespace Analytics.Database.QueryBuilders
{
    public interface IFunnelQueryComposer
    {
        IFunnelQueryComposer WithIndexes(IEnumerable<string> indexes);
        IFunnelQueryComposer WithFunnels(Funnel[] funnels);

        Func<SearchDescriptor<ElasticAnalyticsSession>, ISearchRequest> CreateQuery();
        Func<ISearchResponse<ElasticAnalyticsSession>, int[]> CreateResultExtractor();
    }
}
