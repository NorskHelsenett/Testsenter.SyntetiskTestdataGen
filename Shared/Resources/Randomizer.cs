using System;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class Randomizer
    {
        private Random _random;

        private double[] _doubles;
        private int[] _ints;
        private char[] _chars;
        //private int _max;
        private int _doubleNextIndex, _intNextIndex, _charNextIndex;


        public Randomizer()
        {
            _random = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Buildup(int howManyDoubles, int howManyChars, int howManyInts)
        {
            return;
            _doubleNextIndex = 0;
            _intNextIndex = 0;
            _charNextIndex = 0;
            //_max = howMany;

            _doubles = new double[howManyDoubles];
            _ints = new int[howManyInts];
            _chars = new char[howManyChars];

            for (int i = 0; i < howManyDoubles; i++)
            {
                _doubles[i] = _random.NextDouble();
            }
            for (int i = 0; i < howManyInts; i++)
            {
                _ints[i] = _random.Next(100);
            }
            for (int i = 0; i < howManyChars; i++)
            {
                _chars[i] = GetRandomLetter();
            }
        }

        public bool HitPropabilityDecimal(double probability)
        {
            return probability >= 1.0 || NextDouble() <= probability;
        }

        public bool Hit(double probabilityInPercent)
        {
            if (probabilityInPercent < 1.0)
                return HitPropabilityDecimal(probabilityInPercent / 100.0);

            return Hit((int)Math.Round(probabilityInPercent));
        }

        public bool Hit(int probabilityInPercent)
        {
            var myNumber = Next(100);
            return probabilityInPercent > myNumber;
        }

        public int Next(int maxValueNotInclusive)
        {
            //if (maxValueNotInclusive == 100)
            //{
            //    var myIndex = Interlocked.Increment(ref _intNextIndex);
            //    if (myIndex < _ints.Length)
            //        return _ints[myIndex];

            //    Console.WriteLine("Warning: _intNextIndex is over _max. Using locking");
            //}

            //lock (_lockie)
            //{
                return _random.Next(maxValueNotInclusive);
            //}
        }

        public char NextLetter()
        {
            //var myIndex = Interlocked.Increment(ref _charNextIndex);
            //if (myIndex < _chars.Length)
            //    return _chars[myIndex];

            //Console.WriteLine("Warning: _charNextIndex is over _max. Using locking");

            //lock (_lockie)
            //{
                return GetRandomLetter();
            //}
        }

        private char GetRandomLetter()
        {
            int num = _random.Next(0, 26); // Zero to 25
            char let = (char)('a' + num);
            return let;
        }

        public double NextDouble()
        {
            //var myIndex = Interlocked.Increment(ref _doubleNextIndex);
            //if (myIndex < _doubles.Length)
            //    return _doubles[myIndex];

            //Console.WriteLine("Warning: _doubleNextIndex is over _max. Using locking");

            //lock (_lockie)
            //{
                return _random.NextDouble();
            //}
        }
    }
}
