using GameCore.Cards;
using System;
using System.Collections.Generic;

namespace AI.Evolution
{
    static class Distance
    {
        /// <summary>
        /// Returns editation distance of two lists.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CalcLevensteinDistance(this List<(CardType Card, int Number)> a, List<(CardType Card, int Number)> b)
        {
            if (a.Count == 0)
                return b.Count;
            if (b.Count == 0)
                return a.Count;

            int lengthA = a.Count;
            int lengthB = b.Count;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1].Card == a[i - 1].Card ? 0 : 1;
                    distances[i, j] = Math.Min(Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                                                        distances[i - 1, j - 1] + cost);
                }
            return distances[lengthA, lengthB];
        }
    }
}
