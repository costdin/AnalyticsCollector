using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Analytics.Database.QueryBuilders
{
    public class FunnelQueryComposer : IFunnelQueryComposer
    {
        private IEnumerable<string> _indexes;
        private Funnel[] _funnels;
        private string _firstFilterName = "1";
        private string _nestedName = "2";
        private string _elementFilterName = "3";
        private string _scriptedMetricName = "4";
        private string _funnelParameter = "p0";

        public IFunnelQueryComposer WithIndexes(IEnumerable<string> indexes)
        {
            _indexes = indexes;

            return this;
        }

        public IFunnelQueryComposer WithFunnels(Funnel[] funnels)
        {
            _funnels = funnels;

            return this;
        }

        public Func<SearchDescriptor<ElasticAnalyticsSession>, ISearchRequest> CreateQuery()
        {
            var initScript = BuildInitScript();
            var mapScript = BuildMapScript(_funnels);
            var reduceScript = BuildReduceScript();

            return s => s
                .Size(0)
                .Index(Indices.Index(_indexes))
                .Aggregations(a => a.Filter(_firstFilterName, d => FunnelsToInitialFilter(d, _funnels)
                    .Aggregations(q => q.Nested(_nestedName, n => n.Path(na => na.Events)
                        .Aggregations(qq => qq.Filter(_elementFilterName, dd => dd.Filter(f => FunnelsToFilter(f, _funnels))
                            .Aggregations(qqq => qqq.ScriptedMetric(_scriptedMetricName, m => m
                                                   .InitScript(initScript)
                                                   .MapScript(mapScript)
                                                   .ReduceScript(reduceScript)
                                                   .Params(p => p.Add(_funnelParameter, _funnels))))))))));
        }
        
        public Func<ISearchResponse<ElasticAnalyticsSession>, int[]> CreateResultExtractor()
        {
            return response => response.Aggregations
                .Children(_firstFilterName)
                .Children(_nestedName)
                .Children(_elementFilterName)
                .ScriptedMetric(_scriptedMetricName).Value<int[]>();
        }

        private string BuildReduceScript()
        {
            return "int[] result = new int[states[0].funnels.length]; for(s in states) {for(int i=0;i<states[0].funnels.length;i++){ result[i] += s.funnels[i]; } } return result;";
        }

        private string BuildInitScript()
        {
            return $"state.funnels= new int[params['{_funnelParameter}'].length]; state.level=0; state.session=''";
        }

        private string BuildMapScript(Funnel[] funnels)
        {
            var init = "if (state.session!=doc['events.sessionId'][0]){state.level=0;state.session=doc['events.sessionId'][0];}";

            var funnelBlocks = funnels
                .Select((funnel, index) => FunnelToMapScriptBlock(funnel, index, funnels.Length))
                .Aggregate((a, b) => $"{a} else {b}");

            return init + funnelBlocks;
        }

        private FilterAggregationDescriptor<ElasticAnalyticsSession> FunnelsToInitialFilter(
            FilterAggregationDescriptor<ElasticAnalyticsSession> d,
            Funnel[] funnels)
        {
            var funnelFilter = FunnelToQuery(funnels.First());

            return d.Filter(q =>
                    q.Nested(n => n
                        .Path("events")
                        .Query(qq =>
                            qq.Bool(b => b.Filter(funnelFilter)))));
        }

        private QueryContainer FunnelsToFilter(
            QueryContainerDescriptor<ElasticAnalyticsSession> d,
            Funnel[] funnels)
        {
            return funnels
                .Select(f => FunnelToQuery(f))
                .Select(q => q(d))
                .Aggregate((a, b) => a || b);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetElementIdQuery(Funnel funnel)
        {
            return f => f.Term(v => v.Events.First().ElementId, funnel.ElementId);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetPathQuery(Funnel funnel)
        {
            return f => f.Term(v => v.Events.First().Path, funnel.Path);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetEventTypeQuery(Funnel funnel)
        {
            return f => f.Term(v => v.Events.First().EventType, funnel.EventType);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetXRangeQuery(Funnel funnel)
        {
            return GetRangeQuery(v => v.Events.First().X, funnel.XRange);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetYRangeQuery(Funnel funnel)
        {
            return GetRangeQuery(v => v.Events.First().Y, funnel.YRange);
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> GetRangeQuery(
            Expression<Func<ElasticAnalyticsSession, object>> fieldSelector,
            Range range)
        {
            return f => f.Range(r => r.Field(fieldSelector)
                                      .GreaterThanOrEquals(range.Start)
                                      .LessThanOrEquals(range.End));
        }

        private Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer> FunnelToQuery(Funnel funnel)
        {
            var filterList = new List<Func<QueryContainerDescriptor<ElasticAnalyticsSession>, QueryContainer>>();

            if (!string.IsNullOrEmpty(funnel.ElementId))
            {
                filterList.Add(GetElementIdQuery(funnel));
            }

            if (!string.IsNullOrEmpty(funnel.EventType))
            {
                filterList.Add(GetEventTypeQuery(funnel));
            }

            if (!string.IsNullOrEmpty(funnel.Path))
            {
                filterList.Add(GetPathQuery(funnel));
            }

            if (funnel.XRange != null)
            {
                filterList.Add(GetXRangeQuery(funnel));
            }

            if (funnel.YRange != null)
            {
                filterList.Add(GetYRangeQuery(funnel));
            }

            return f => filterList
                         .Select(fieldQuery => fieldQuery(f))
                         .Aggregate((a, b) => a && b);
        }

        private string FunnelToMapScriptBlock(Funnel funnel, int index, int funnelCount)
        {
            var conditions = new List<string>();
            if (index > 0)
            {
                conditions.Add($"state.level=={index}");
            }

            if (!string.IsNullOrEmpty(funnel.ElementId))
            {
                conditions.Add($"params['{_funnelParameter}'][{index}].elementId==doc['events.elementId'][0]");
            }

            if (!string.IsNullOrEmpty(funnel.EventType))
            {
                conditions.Add($"params['{_funnelParameter}'][{index}].eventType==doc['events.eventType'][0]");
            }

            if (!string.IsNullOrEmpty(funnel.Path))
            {
                conditions.Add($"params['{_funnelParameter}'][{index}].path==doc['events.path'][0]");
            }

            if (funnel.XRange != null)
            {
                conditions.Add($"params['{_funnelParameter}'][{index}].xRange.start<=doc['events.x'][0] && params['{_funnelParameter}'][{index}].xRange.end>=doc['events.x'][0]");
            }

            if (funnel.YRange != null)
            {
                conditions.Add($"params['{_funnelParameter}'][{index}].yRange.start<=doc['events.y'][0] && params['{_funnelParameter}'][{index}].yRange.end>=doc['events.y'][0]");
            }

            var ifCondition = conditions.Aggregate((a, b) => $"({a}) && ({b})");
            var ifBody = $"state.funnels[{index}]++;state.level={(index + 1) % funnelCount};";

            var result = $"if ({ifCondition}){{{ifBody}}}";

            return result;
        }
    }
}
