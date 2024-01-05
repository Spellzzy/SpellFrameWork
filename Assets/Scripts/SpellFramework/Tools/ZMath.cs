using System;

namespace SpellFramework.Tools
{
    public class ZMath
    {
        private static Random _random;

        private static Random SRandom
        {
            get
            {
                if (_random != null) return _random;
                _random = new Random((int)DateTime.Now.Ticks);

                return _random;
            }
        }
        
        public static float Random()
        {
            return (float)SRandom.NextDouble();
        }

        public static int Random(int max)
        {
            return SRandom.Next(max);
        }

        public static float Random(float max)
        {
            return (float)(SRandom.NextDouble() * max);
        }

        public static int Random(int min, int max)
        {
            return min + SRandom.Next(max - min);
        }
        
        public static float Random(float min, float max)
        {
            return (float)(min + ((max - min) * SRandom.NextDouble()));
        }
    }
}