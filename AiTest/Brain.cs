using System;
using System.Collections.Generic;

namespace AiTest
{
    public class Brain
    {
        private const int MaxDirectionChange = 60;
        private static readonly Random random = new Random();

        private readonly int count;

        private List<int> directions;
        private int step;

        public Brain(int count)
        {
            this.count = count;
            directions = new List<int>();
            for (var i = 0; i < count; i++)
            {
                directions.Add(random.Next(MaxDirectionChange) - (MaxDirectionChange / 2));
            }
        }

        public int? NextDirection()
        {
            if (step >= count)
            {
                return null;
            }

            return directions[step++];
        }
    }
}
