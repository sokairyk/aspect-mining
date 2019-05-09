using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class CollectionExtensions
    {
        public static List<List<T>> GetAllCombos<T>(this List<T> initialList)
        {
            var ret = new List<List<T>>();

            // The final number of sets will be 2^N (or 2^N - 1 if skipping empty set)
            int setCount = Convert.ToInt32(Math.Pow(2, initialList.Count()));

            // Start at 1 if you do not want the empty set
            for (int mask = 0; mask < setCount; mask++)
            {
                var nestedList = new List<T>();
                for (int j = 0; j < initialList.Count(); j++)
                {
                    // Each position in the initial list maps to a bit here
                    var pos = 1 << j;
                    if ((mask & pos) == pos) { nestedList.Add(initialList[j]); }
                }
                ret.Add(nestedList);
            }
            return ret;
        }

    }
}
