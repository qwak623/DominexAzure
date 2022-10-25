using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Subsets : IEnumerable<List<int>>
    {
        List<int> numbers;
        int size;

        public Subsets(List<int> numbers, int size)
        {
            this.numbers = numbers;
            this.size = size;
        }

        public IEnumerator<List<int>> GetEnumerator()
        {
            long c = 2 << numbers.Count - 1;

            for (long i = 0; i < c; i++)
            {
                List<int> result = new List<int>();
                long x = i;
                int a = 0;
                while (x > 0)
                {
                    if (x % 2 == 1)
                        result.Add(numbers[a]);
                    x /= 2;
                    a++;
                }

                if (result.Count == size)
                    yield return result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
