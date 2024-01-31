using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



    public class UtilityFunctions
    {
        public double[][] InitializeSolutions(ABC abcInstance)
        {
            double[][] solutions = new double[abcInstance.totalBees][];
            for (int i = 0; i < abcInstance.totalBees; i++)
            {
                solutions[i] = new double[abcInstance.dimensions];
                for (int j = 0; j < abcInstance.dimensions; j++)
                {
                    solutions[i][j] = abcInstance.random.NextDouble() * (abcInstance.upperBound[j] - abcInstance.lowerBound[j]) + abcInstance.lowerBound[j];
                }
            }
            return solutions;
        }

        public double[] EvaluateSolutions(double[][] solutions, Func<double[], double> objectiveFunction, ABC abcInstance)
        {
            double[] fitness = new double[abcInstance.totalBees];
            for (int i = 0; i < abcInstance.totalBees; i++)
            {
                fitness[i] = objectiveFunction(solutions[i]);
            }
            return fitness;
        }

        public  int GetRandomNeighbor(int currentIndex, int maxIndex, ABC abcInstance)
        {
            int neighborIndex;
            do
            {
                neighborIndex = abcInstance.random.Next(maxIndex);
            } while (neighborIndex == currentIndex);

            return neighborIndex;
        }

        public  int SelectBee(double[] probabilities, ABC abcInstance)
        {
            double randomValue = abcInstance.random.NextDouble();
            double cumulativeProbability = 0.0;

            for (int i = 0; i < abcInstance.totalBees; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomValue < cumulativeProbability)
                {
                    return i;
                }
            }

            return abcInstance.totalBees - 1;
        }

        public int GetBestSolutionIndex(double[] fitness)
        {
            double minFitness = fitness.Min();
            return Array.IndexOf(fitness, minFitness);
        }

        public  double Clamp(double value, double min, double max)
        {
            return Math.Max(min, Math.Min(max, value));
        }
    public static UtilityFunctions Instance { get; } = new UtilityFunctions();
}
