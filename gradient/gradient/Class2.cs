using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class UtilityFunctions
{
    private static Random randoms = new Random();

    public static double[] GetRandomInitialGuess(int dimensions, double[] lowerBound, double[] upperBound)
   {
       double[] initialGuess = new double[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            initialGuess[i] = randoms.NextDouble() * (upperBound[i] - lowerBound[i]) + lowerBound[i];
        }
        return initialGuess;
    }

    // Pozostałe funkcje pomocnicze...
}
