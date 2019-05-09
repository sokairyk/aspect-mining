using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class Levenshtein
    {
        //*****************************
        // Compute Levenshtein distance 
        // Memory efficient version
        //*****************************
        public static double LevenshteinDistance(this string source, string target)
        {
            int RowLen = source.Length;  // length of source
            int ColLen = target.Length;  // length of target
            int RowIdx;                  // iterates through source
            int ColIdx;                  // iterates through target
            char Row_i;                  // ith character of source
            char Col_j;                  // jth character of target
            int cost;                    // cost

            // Test string length
            if (Math.Max(source.Length, target.Length) > Math.Pow(2, 31))
                throw (new Exception("\nMaximum string length in Levenshtein.iLD is " + Math.Pow(2, 31) + ".\nYours is " + Math.Max(source.Length, target.Length) + "."));

            // Step 1
            if (RowLen == 0) return ColLen;

            if (ColLen == 0) return RowLen;

            // Create the two vectors
            int[] v0 = new int[RowLen + 1];
            int[] v1 = new int[RowLen + 1];
            int[] vTmp;

            // Step 2
            // Initialize the first vector
            for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
            {
                v0[RowIdx] = RowIdx;
            }

            // Step 3
            // Fore each column
            for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
            {
                // Set the 0'th element to the column number
                v1[0] = ColIdx;

                Col_j = target[ColIdx - 1];

                // Step 4
                // Fore each row
                for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                {
                    Row_i = source[RowIdx - 1];

                    // Step 5
                    if (Row_i == Col_j)
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }

                    // Step 6
                    // Find minimum
                    int m_min = v0[RowIdx] + 1;
                    int b = v1[RowIdx - 1] + 1;
                    int c = v0[RowIdx - 1] + cost;

                    if (b < m_min)
                    {
                        m_min = b;
                    }
                    if (c < m_min)
                    {
                        m_min = c;
                    }

                    v1[RowIdx] = m_min;
                }

                // Swap the vectors
                vTmp = v0;
                v0 = v1;
                v1 = vTmp;
            }

            // Step 7
            // Value between 0 - 100
            // 0==perfect match 100==totaly different
            // 
            // The vectors where swaped one last time at the end of the last loop,
            // that is why the result is now in v0 rather than in v1
            //int max = Math.Max(RowLen, ColLen);
            return v0[RowLen];
        }

        public static double NormalizedLevenshteinDistance(this string source, string target)
        {
            double unnormalizedLevenshteinDistance = source.LevenshteinDistance(target);

            return 2 * unnormalizedLevenshteinDistance / (source.Length + target.Length + unnormalizedLevenshteinDistance);
        }
    }
}
