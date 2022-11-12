using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalyticsCollector.Library
{
    public class FunnelFinder<T>
    {
        public int[] Find(T[] list, T[] funnel)
        {
            int[] result = new int[funnel.Length];

            var position = Array.IndexOf(list, funnel[0]);

            int currentLevel = 0;
            while (position >= 0)
            {
                result[currentLevel]++;
                currentLevel++;
                currentLevel = currentLevel % funnel.Length;

                position = Array.IndexOf(list, funnel[currentLevel], position + 1);
            }

            return result;
        }
    }
}
