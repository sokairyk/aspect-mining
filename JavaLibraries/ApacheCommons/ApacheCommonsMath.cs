using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using org.apache.commons.math3.stat.interval;

namespace JavaLibraries.ApacheCommons
{
    public class ApacheCommonsMath
    {
        private static readonly WilsonScoreInterval WilsonScore =  new WilsonScoreInterval();

        public static double GetWilsonScore(int total, int positive, double confidence)
        {
            return WilsonScore.createInterval(total, positive, confidence).getLowerBound();
        }
    }
}
