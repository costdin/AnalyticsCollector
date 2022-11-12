using Nest;
using System;
using System.Collections.Generic;

namespace Analytics.Database.QueryBuilders
{
    public interface IFunnelQueryBuilder
    {
        IFunnelQueryComposer Build();
    }
}