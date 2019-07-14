using System;

namespace AiTest
{
    public class VelocityVector
    {
        public int Direction { get; set; }
        public double Speed { get; set; }
        public double DirectionInRadians => DegreesToRadians(Direction);

        private double DegreesToRadians(int degrees) => (Math.PI / 180.0) * degrees;
    }
}
