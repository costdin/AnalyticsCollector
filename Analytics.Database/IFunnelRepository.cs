using System.Threading.Tasks;

namespace Analytics.Database
{
    public interface IFunnelRepository
    {
        Task<int[]> FunnelAnalysis(Funnel[] funnels, params string[] indexList);
    }
}