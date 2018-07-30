using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RDotNet;

namespace Correlation
{
    public static class RIntegration
    {
        private static REngine _engine;

        public static void StartEngine()
        {
            SetupPath();

            _engine = REngine.CreateInstance("RDotNet");
            _engine.Initialize();
            _engine.Evaluate("library(bindata)");
            _engine.Evaluate("library(Matrix)");
        }

        public static void StopEngine()
        {
            _engine.Dispose();
        }

        public static double[,] GetSigma(double[,] commonProb)
        {
            var commonprobStr = "cbind(";
            for (int i = 0; i < commonProb.GetLength(0); i++)
            {
                var thisStr = "c(";
                for (int j = 0; j < commonProb.GetLength(0); j++)
                {
                    thisStr += commonProb[i, j];
                    if (j != commonProb.GetLength(0) - 1)
                        thisStr += ",";
                }
                thisStr += ")";
                commonprobStr += thisStr;
                if (i != commonProb.GetLength(0) - 1)
                    commonprobStr += ",";
            }
            commonprobStr += ")";

            double[,] result = new double[commonProb.GetLength(0), commonProb.GetLength(0)];

            var b = _engine.Evaluate($"commonprob2sigma({commonprobStr})").AsNumericMatrix();
            var numberOfNegativeEigenvalues = _engine.Evaluate($"sum(eigen(commonprob2sigma({commonprobStr}))$values < 0)").AsInteger();
            bool hasNegativeEigenvalues = numberOfNegativeEigenvalues[0] > 0;

            if (hasNegativeEigenvalues)
            {

                result = new double[commonProb.GetLength(0), commonProb.GetLength(0)];
                var oneDimMatrix = _engine.Evaluate($"nearPD(commonprob2sigma({commonprobStr}))$mat@x").AsNumericMatrix();
                int counter = 0;
                for (int i = 0; i < commonProb.GetLength(0); i++)
                {
                    for (int j = 0; j < commonProb.GetLength(0); j++)
                    {
                        result[i, j] = oneDimMatrix[counter++, 0];
                    }
                }
            }
            else
                b.CopyTo(result, commonProb.GetLength(0), commonProb.GetLength(0));


            return result;
        }

        private static void SetupPath()
        {
            var oldPath = System.Environment.GetEnvironmentVariable("PATH");
            var rPath = @"C:\Program Files\R\R-3.4.1\bin\i386";

            if (!Directory.Exists(rPath))
                throw new DirectoryNotFoundException(string.Format("R.dll not found in : {0}", rPath));

            var containsAlready = oldPath.Contains(rPath);
            if (containsAlready)
                return;

            var newPath = string.Format("{0}{1}{2}", rPath, System.IO.Path.PathSeparator, oldPath);

            System.Environment.SetEnvironmentVariable("PATH", newPath);
        }
    }
}
