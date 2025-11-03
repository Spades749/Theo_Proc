using UnityEngine;

namespace VTools.RandomService
{
    public class RandomService
    {
        public System.Random Random { get; }
        public int Seed { get; }

        public RandomService(int seed)
        {
            Random = new System.Random(seed);
            Seed = seed;
        }
        
        // ----------------------------------------- METHODS -------------------------------------------------

        /// Returns a random integer between min [inclusive] and max [exclusive]
        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                Debug.LogError("[RandomService] Range(maxExclusive <= minInclusive).");
                return minInclusive;
            }

            return Random.Next(minInclusive, maxExclusive);
        }

        /// Returns a random float between min [inclusive] and max [inclusive]
        public float Range(float minInclusive, float maxInclusive)
        {
            if (maxInclusive < minInclusive)
            {
                Debug.LogError("[RandomService] Range(maxInclusive < minInclusive).");
                return minInclusive;
            }

            double t = Random.NextDouble();  // returns [0.0, 1.0)
            return (float)(minInclusive + (t * (maxInclusive - minInclusive)));
        }

        /// Returns true with a given probability (0–1)
        public bool Chance(float probability)
        {
            if (probability <= 0f) return false;
            if (probability >= 1f) return true;

            return Random.NextDouble() < probability;
        }

        /// Returns a random element from a list or array (safe against empty collections)
        public T Pick<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogError("[RandomService] Pick called on empty array.");
                return default;
            }

            int index = Random.Next(0, array.Length);
            return array[index];
        }
    }
}