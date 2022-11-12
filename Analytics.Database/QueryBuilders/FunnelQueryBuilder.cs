namespace Analytics.Database.QueryBuilders
{
    public class FunnelQueryBuilder : IFunnelQueryBuilder
    {
        public IFunnelQueryComposer Build()
        {
            return new FunnelQueryComposer();
        }
    }
}
