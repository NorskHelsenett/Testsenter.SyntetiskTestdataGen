using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Statistics.Properties;

namespace SyntetiskTestdataGen.Shared.Statistics
{
    public class CorrelationMatrix
    {
        public MatrixElement[,] Matrix { get; set; }
        private Dictionary<string, int> _axisKeys;

        public CorrelationMatrix()
        {
        }

        public CorrelationMatrix(int length, string[] headers)
        {
            Matrix = new MatrixElement[length+1,length+1];
            _axisKeys = new Dictionary<string, int>();

            for (int i = 0; i < headers.Length; i++)
            {
                Matrix[0, i + 1] = new MatrixElement {ValuesAsString = headers[i]};
                Matrix[i + 1, 0] = new MatrixElement {ValuesAsString = headers[i]};

                _axisKeys.Add(headers[i], i + 1);
            }
        }

        public double SetValue(string xAxisName, string yAxisName, double? value, int? numberOfSamples, double correlationThreshold = 0.2)
        {
            var x = _axisKeys[xAxisName];
            var y = _axisKeys[yAxisName];

            Matrix[x,y] = new MatrixElement
            {
                XAxisName = xAxisName,
                YAxisName = yAxisName,
                ValuesAsString = (value?.ToString(CultureInfo.InvariantCulture) ?? "null") + (numberOfSamples.HasValue ? "(Antall=" + numberOfSamples.Value + ")" : ""),
                Value = value ?? 0
            };

            return Matrix[x, y].Value;
        }

        public void PutValue(string xAxisName, string yAxisName, double newValue)
        {
            if (_axisKeys == null)
                SetAxisKeys();

            var x = _axisKeys[xAxisName];
            var y = _axisKeys[yAxisName];

            Matrix[x, y].Value = newValue;
        }

        public MatrixElement GetValue(string xAxisName, string yAxisName)
        {
            if (_axisKeys == null)
                SetAxisKeys();

            var x = _axisKeys[xAxisName];
            var y = _axisKeys[yAxisName];

            if(Matrix[x, y] == null)
                Matrix[x, y] = new MatrixElement
                {
                    XAxisName = xAxisName,
                    YAxisName = yAxisName,
                    ValuesAsString = "",
                    Value =  0
                };


            return Matrix[x, y];
        }

        private void SetAxisKeys()
        {
            _axisKeys = new Dictionary<string, int>();

            for (int i = 1; i < Matrix.GetLength(0); i++)
            {
                _axisKeys.Add(Matrix[0, i].ValuesAsString, i);
            }
        }

        public void Closure(CorrelationFactory correlationFact)
        {
            for (int vertical = 0; vertical < Matrix.GetLength(0); vertical++)
            {
                for (int horizontal = 0; horizontal < Matrix.GetLength(0); horizontal++)
                {
                    var element = Matrix[vertical, horizontal];
                    if(element == null || string.IsNullOrEmpty(element.XAxisName) || string.IsNullOrEmpty(element.YAxisName))
                        continue;

                    element.Value /= (correlationFact.CorrelationFactoryElements[element.XAxisName].GetStdDev() * correlationFact.CorrelationFactoryElements[element.YAxisName].GetStdDev());
                }
            }
        }

        public void Update(Person p, int ageQuant, Dictionary<string, Statistic> statisticsDic, double correlationThreshold = 0.3)
        {
            var taken = new HashSet<string>();
            var round2Ok = new HashSet<string>();

            var listOfKeys = statisticsDic.Where(v => v.Value.ApplicableForCorrelation()).Select(x => x.Key).ToList();

            for (int i=0; i<listOfKeys.Count; i++) //vertical
            {
                var outerStatKey = listOfKeys.ElementAt(i);
                for (int j=0; j<listOfKeys.Count; j++) //horizont
                {
                    var innerStatKey = listOfKeys.ElementAt(j);
                    if (taken.Contains(outerStatKey + innerStatKey) || taken.Contains(innerStatKey + outerStatKey))
                        continue;

                    var outerStat = statisticsDic[outerStatKey];
                    var innerStat = statisticsDic[innerStatKey];

                    taken.Add(outerStatKey + innerStatKey);

                    var outerValue = outerStat.GetValue(p, ageQuant, false);
                    var innerValue = innerStat.GetValue(p, ageQuant, false);

                    if (outerValue == null || innerValue == null)
                        continue;

                    DoRound2(round2Ok, outerStatKey, outerStat, outerValue);
                    DoRound2(round2Ok, innerStatKey, innerStat, innerValue);

                    var currentValue = GetValue(outerStatKey, innerStatKey).Value;

                    var valueToAdd = 
                        (outerValue.Value - outerStat.CorrelationFactory.CorrelationFactoryElements[outerStatKey].GetAverage()) 
                        * 
                        (innerValue.Value - innerStat.CorrelationFactory.CorrelationFactoryElements[innerStatKey].GetAverage());

                    valueToAdd /= (Math.Min(outerStat.CorrelationFactory.CorrelationFactoryElements[outerStatKey].Round1Samples, innerStat.CorrelationFactory.CorrelationFactoryElements[innerStatKey].Round1Samples) - 1);

                    currentValue += valueToAdd;

                    PutValue(outerStatKey, innerStatKey, currentValue);
                }
            }
        }

        private void DoRound2(HashSet<string> round2Ok, string outerStatKey, Statistic outerStat, double? outerValue)
        {
            if (round2Ok.Contains(outerStatKey) || !outerValue.HasValue)
                return;

            outerStat.CorrelationFactory.Round2(outerStatKey, outerValue.Value);

            round2Ok.Add(outerStatKey);
        }

        public static CorrelationMatrix Create(Dictionary<string, Statistic> statisticsDic, double correlationThreshold = 0.3)
        {
            var matrix = new CorrelationMatrix(statisticsDic.Count, statisticsDic.Keys.ToArray());
            var taken = new HashSet<string>();

            foreach (var outerStat in statisticsDic.Keys) //vertical
            {
                foreach (var innerStat in statisticsDic.Keys) //horizont
                {
                    if(taken.Contains(outerStat + innerStat) || taken.Contains(innerStat + outerStat))
                        continue;

                    var valuesOuter = statisticsDic[outerStat].GetSamples();
                    var valuesInner = statisticsDic[innerStat].GetSamples();

                    if (valuesInner == null || valuesOuter == null || valuesOuter.Length < 2 || valuesInner.Length < 2)
                    {
                        matrix.SetValue(outerStat, innerStat, null, null);
                        continue;
                    }

                    int? numberOfSamples = null;
                    if (valuesOuter.Length != valuesInner.Length)
                    {
                        numberOfSamples = Math.Min(valuesInner.Length, valuesOuter.Length);
                        valuesInner = valuesInner.Take(numberOfSamples.Value).ToArray();
                        valuesOuter = valuesOuter.Take(numberOfSamples.Value).ToArray();
                    }

                    var value = matrix.SetValue(outerStat, innerStat, StatisticHelper.CalculateCorrelation(valuesOuter, valuesInner), numberOfSamples);
                    taken.Add(outerStat + innerStat);
                }
            }

            return matrix;
        }

        public void Output()
        {
            bool isFirstLine = true;

            for (int vertical = 0; vertical < Matrix.GetLength(0); vertical++)
            {
                var thisLine = "";
                for (int horizontal = 0; horizontal < Matrix.GetLength(0); horizontal++)
                {
                    if (thisLine != "")
                        thisLine += ",";

                    var element = Matrix[vertical, horizontal];
                    if (element == null)
                        thisLine += "";
                    else
                        thisLine += element.ValuesAsString;

                }

                if (isFirstLine) 
                    thisLine = "," + thisLine;

                isFirstLine = false;
                Console.WriteLine(thisLine);
            }
        }
    }

    public class MatrixElement
    {
        public string XAxisName { get; set; }
        public string YAxisName { get; set; }
        public string ValuesAsString { get; set; }
        public double Value { get; set; }
    }
}
