using System;
using System.Collections.Generic;
using System.Linq;

namespace AiTest
{
    public class Brain
    {
        private const int MaxDirectionChange = 30;
        private const double MutationRate = 0.01d;
        private static readonly Random random = new Random();

        private readonly int count;

        public List<int> Directions { get; }

        public int Step { get; private set; }

        public Brain(int count)
            : this(count, GenerateDirections(count))
        {
        }

        private Brain(int count, List<int> directions)
        {
            this.count = count;
            Directions = directions;
        }

        private static List<int> GenerateDirections(int count)
        {
            var directions = new List<int>();
            for (var i = 0; i < count; i++)
            {
                directions.Add(RandomDirection);
            }
            return directions;
        }

        private static int RandomDirection => random.Next(MaxDirectionChange) - (MaxDirectionChange / 2);

        public int? NextDirection()
        {
            if (Step >= count)
            {
                return null;
            }

            return Directions[Step++];
        }

        public Brain Clone() => new Brain(count, Directions.ToList());

        public void Mutate()
        {
            for (int i = 0; i < count; i++)
            {
                var r = random.NextDouble();
                if (r < MutationRate)
                {
                    Directions[i] = RandomDirection;
                }
            }
        }
    }
}
