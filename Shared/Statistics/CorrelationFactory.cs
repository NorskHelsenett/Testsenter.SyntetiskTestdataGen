using System;
using System.Collections.Generic;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public class CorrelationFactory 
    {
        public Dictionary<string, CorrelationFactoryElement> CorrelationFactoryElements { get; set; }

        public CorrelationFactory()
        {
            CorrelationFactoryElements = new Dictionary<string, CorrelationFactoryElement>();
        }

        public void Round1(string key, double value)
        {
            EnsureExists(key);

            if (CorrelationFactoryElements[key].Round1Overflown)
                return;

            try
            {
                CorrelationFactoryElements[key].RunningAverage += value;
                CorrelationFactoryElements[key].Round1Samples++;
            }
            catch (OverflowException)
            {
                CorrelationFactoryElements[key].Round1Overflown = true;
                return;
            }
        }

        public void Round2(string key, double value)
        {
            EnsureExists(key);

            if (CorrelationFactoryElements[key].Round2Overflown)
                return;

            bool firstSuccess = false;
            try
            {
                CorrelationFactoryElements[key].RunningTotalSum += (value - CorrelationFactoryElements[key].GetAverage());
                firstSuccess = true;

                var valueInPowerOfTwo = ((double) (Math.Pow(value - CorrelationFactoryElements[key].GetAverage(), 2)));
                CorrelationFactoryElements[key].RunningStdDev += valueInPowerOfTwo / ((double) CorrelationFactoryElements[key].Round1Samples);
            }
            catch (OverflowException)
            {
                CorrelationFactoryElements[key].Round2Overflown = true;
                if(firstSuccess)
                    CorrelationFactoryElements[key].RunningTotalSum -= (value - CorrelationFactoryElements[key].GetAverage());
            }
        }

        private void EnsureExists(string key)
        {
            if (!CorrelationFactoryElements.ContainsKey(key))
                CorrelationFactoryElements.Add(key, new CorrelationFactoryElement() { Key = key });
        }
    }

    public class CorrelationFactoryElement
    {
        public string Key { get; set; } 
        public double RunningAverage { get; set; }
        public double RunningTotalSum { get; set; }
        public double RunningStdDev { get; set; }
        public int Round1Samples { get; set; }
        public int Round2Samples { get; set; }
        public bool Round1Overflown { get; set; }
        public bool Round2Overflown { get; set; }

        private double? _average;
        public double GetAverage()
        {
            if(!_average.HasValue)
                _average = RunningAverage / Round1Samples;

            return _average.Value;
        }

        public double GetStdDev()
        {
            return Math.Sqrt(RunningStdDev);
        }
    }
}
