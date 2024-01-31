using System;

using System.Linq;
using System.Security.Cryptography;
using System;
using System.IO;
public class SnakeOptimizer
{
    public static int totalBees = 40;
    public static int dimensions = 3;
    public static double lowerBound = -10.0;
    public static double upperBound = 10.0;
    public static int maxGenerations = 100;
    public static int t = 0;
    public static double[] population = new double[totalBees * dimensions];
    public static double[] nMale = new double[totalBees * dimensions / 2];
    public static double[] nFemale = new double[totalBees * dimensions / 2];
    public static double[] mbest = new double[dimensions];
    public static double mbest_fitness = double.MaxValue;
    public static double[] fbest = new double[dimensions];
    public static double fbest_fitness = double.MaxValue;
    public static double[][] initialPoints = new double[totalBees][];
    public static Func<double[], double> objectiveFunction = RastriginFunction.Evaluate;
    


    public static void initializePopulation(double[][] initialPoints)
    {
        Random rand = new Random();
        if (initialPoints.Length != totalBees || initialPoints[0].Length != dimensions)
        {
            throw new ArgumentException("The dimension of the initial points does not match the space dimension.");
        }


        for (int i = 0; i < totalBees * dimensions; i++)
        {
            population[i] = lowerBound + (upperBound - lowerBound) * rand.NextDouble();

        }
        for (int i = 0; i < totalBees; i++)
        {
            Array.Copy(initialPoints[i], 0, population, i * dimensions, dimensions);
            Array.Copy(initialPoints[i], 0, nMale, i * dimensions / 2, dimensions / 2);
            Array.Copy(initialPoints[i], dimensions / 2, nFemale, i * dimensions / 2, dimensions / 2);
        }
    }

    public static (double Fitness, double[] Solution) RunOptimization()
    {

        Random rand = new Random();
        for (int i = 0; i < totalBees; i++)
        {
            initialPoints[i] = new double[dimensions];
            for (int j = 0; j < dimensions; j++)
            {
                initialPoints[i][j] = lowerBound + (upperBound - lowerBound) * rand.NextDouble();

            }
        }
        
        initializePopulation(initialPoints);
        double[] mbest = new double[dimensions];
        double mbest_fitness = double.MaxValue;
        double[] fbest = new double[dimensions];
        double fbest_fitness = double.MaxValue;
        Random random = new Random();
        while (t <= maxGenerations)
        {
            // EVALUATE EACH GROUP
            double[] mFitness = new double[nMale.Length / dimensions];
            double[] fFitness = new double[nFemale.Length / dimensions];

            for (int i = 0; i < nMale.Length / dimensions; i++)
            {
                double[] mPoint = new double[dimensions];
                double[] fPoint = new double[dimensions];

                Array.Copy(nMale, i * dimensions, mPoint, 0, dimensions);
                Array.Copy(nFemale, i * dimensions, fPoint, 0, dimensions);

                mFitness[i] = objectiveFunction(mPoint);
                fFitness[i] = objectiveFunction(fPoint);


            }
            // Oblicz fitness na nowo w każdej iteracji
            for (int i = 0; i < nMale.Length / dimensions; i++)
            {
                if (mFitness[i] < mbest_fitness)
                {
                    Array.Copy(nMale, i * dimensions, mbest, 0, dimensions);
                    mbest_fitness = mFitness[i];
                }
            }

            for (int i = 0; i < nFemale.Length / dimensions; i++)
            {
                if (fFitness[i] < fbest_fitness)
                {
                    Array.Copy(nFemale, i * dimensions, fbest, 0, dimensions);
                    fbest_fitness = fFitness[i];
                }
            }



            // FIND BEST MALE AND FEMALE
            int mbest_index = Array.IndexOf(mFitness, mFitness.Min());
            Array.Copy(nMale, mbest_index * dimensions, mbest, 0, dimensions);
            mbest_fitness = mFitness[mbest_index];

            int fbest_index = Array.IndexOf(fFitness, fFitness.Min());
            Array.Copy(nFemale, fbest_index * dimensions, fbest, 0, dimensions);
            fbest_fitness = fFitness[fbest_index];

            // FIND WORST MALE AND FEMALE
            int mworst_index = Array.IndexOf(mFitness, mFitness.Max());
            double[] mworst = new double[dimensions];
            Array.Copy(nMale, mworst_index * dimensions, mworst, 0, dimensions);

            int fworst_index = Array.IndexOf(fFitness, fFitness.Max());
            double[] fworst = new double[dimensions];
            Array.Copy(nFemale, fworst_index * dimensions, fworst, 0, dimensions);

            // DEFINE TEMP
            double temp = Math.Exp(-t / maxGenerations);

            // DEFINE FOOD QUANTITY
            double food = 0.5 * Math.Exp((t - maxGenerations) / maxGenerations);

            if (food < 0.25)
            {
                // EXPLORATION MODE
                double c2 = 0.05;

                int randomIndexM = random.Next(0, nMale.Length / dimensions);
                double amm = 0.0;
                if (mFitness[t] != 0)
                {
                    amm = Math.Exp(-mFitness[randomIndexM] / mFitness[t]);
                }

                double[] updatedXm = new double[dimensions];
                Array.Copy(nMale, randomIndexM * dimensions, updatedXm, 0, dimensions);

                for (int i = 0; i < dimensions; i++)
                {
                    //updatedXm[i] += c2 * amm * (ub - lb) * (2 * random.NextDouble() - 1);          
                    updatedXm[i] += c2 * amm * ((upperBound - lowerBound) * random.NextDouble() + lowerBound);

                }

                Array.Copy(updatedXm, 0, nMale, randomIndexM * dimensions, dimensions);

                int randomIndexF = random.Next(0, nFemale.Length / dimensions);
                double amf = 0.0;
                if (fFitness[t] != 0)
                {
                    amf = Math.Exp(-fFitness[randomIndexF] / fFitness[t]);
                }

                double[] updatedXf = new double[dimensions];
                Array.Copy(nFemale, randomIndexF * dimensions, updatedXf, 0, dimensions);

                for (int i = 0; i < dimensions; i++)
                {
                    //updatedXf[i] += c2 * amf * (ub - lb) * (2 * random.NextDouble() - 1);
                    updatedXf[i] += c2 * amf * ((upperBound - lowerBound) * random.NextDouble() + lowerBound);

                }

                Array.Copy(updatedXf, 0, nFemale, randomIndexF * dimensions, dimensions);
            }
            else if (food > 0.6)
            {
                // EXPLOITATION MODE
                double c3 = 2;

                for (int i = 0; i < nMale.Length / dimensions; i++)
                {
                    double[] updatedXm = new double[dimensions];
                    Array.Copy(mbest, 0, updatedXm, 0, dimensions);

                    for (int j = 0; j < dimensions; j++)
                    {
                        updatedXm[j] += c3 * temp * random.NextDouble() * (mbest[j] - nMale[i * dimensions + j]);


                    }

                    Array.Copy(updatedXm, 0, nMale, i * dimensions, dimensions);
                }

                for (int i = 0; i < nFemale.Length / dimensions; i++)
                {
                    double[] updatedXf = new double[dimensions];
                    Array.Copy(fbest, 0, updatedXf, 0, dimensions);

                    for (int j = 0; j < dimensions; j++)
                    {
                        updatedXf[j] += c3 * temp * random.NextDouble() * (fbest[j] - nFemale[i * dimensions + j]);

                    }

                    Array.Copy(updatedXf, 0, nFemale, i * dimensions, dimensions);
                }
            }
            else
            {
                if (random.NextDouble() > 0.6)
                {
                    // FIGHT MODE
                    double c3 = 2;

                    for (int i = 0; i < nMale.Length / dimensions; i++)
                    {
                        double fm = 0.0;
                        if (mFitness[i] != 0)
                        {
                            fm = Math.Exp(-fbest_fitness / mFitness[i]);
                        }

                        double[] updatedXm = new double[dimensions];
                        Array.Copy(nMale, i * dimensions, updatedXm, 0, dimensions);

                        for (int j = 0; j < dimensions; j++)
                        {
                            updatedXm[j] += c3 * fm * random.NextDouble() * (food * fbest[j] - nMale[i * dimensions + j]);

                        }

                        Array.Copy(updatedXm, 0, nMale, i * dimensions, dimensions);
                    }

                    for (int i = 0; i < nFemale.Length / dimensions; i++)
                    {
                        double ff = 0.0;
                        if (fFitness[i] != 0)
                        {
                            ff = Math.Exp(-mbest_fitness / fFitness[i]);
                        }

                        double[] updatedXf = new double[dimensions];
                        Array.Copy(nFemale, i * dimensions, updatedXf, 0, dimensions);

                        for (int j = 0; j < dimensions; j++)
                        {
                            updatedXf[j] += c3 * ff * random.NextDouble() * (food * mbest[j] - nFemale[i * dimensions + j]);


                        }

                        Array.Copy(updatedXf, 0, nFemale, i * dimensions, dimensions);
                    }
                }
                else
                {
                    // MATING MODE
                    double c3 = 2;

                    for (int i = 0; i < nMale.Length / dimensions; i++)
                    {
                        double mm = 0.0;
                        if (mFitness[i] != 0)
                        {
                            mm = Math.Exp(-fFitness[i] / mFitness[i]);
                        }

                        double[] updatedXm = new double[dimensions];
                        Array.Copy(nMale, i * dimensions, updatedXm, 0, dimensions);

                        for (int j = 0; j < dimensions; j++)
                        {
                            updatedXm[j] += c3 * mm * random.NextDouble() * (food * nFemale[i * dimensions + j] - nMale[i * dimensions + j]);

                        }

                        Array.Copy(updatedXm, 0, nMale, i * dimensions, dimensions);
                    }

                    for (int i = 0; i < nFemale.Length / dimensions; i++)
                    {
                        double mf = 0.0;
                        if (fFitness[i] != 0)
                        {
                            mf = Math.Exp(-mFitness[i] / fFitness[i]);
                        }

                        double[] updatedXf = new double[dimensions];
                        Array.Copy(nFemale, i * dimensions, updatedXf, 0, dimensions);

                        for (int j = 0; j < dimensions; j++)
                        {
                            updatedXf[j] += c3 * mf * random.NextDouble() * (food * nMale[i * dimensions + j] - nFemale[i * dimensions + j]);

                        }

                        Array.Copy(updatedXf, 0, nFemale, i * dimensions, dimensions);
                    }

                    for (int i = 0; i < dimensions; i++)
                    {

                        mworst[i] = lowerBound + (upperBound - lowerBound) * random.NextDouble();
                        fworst[i] = lowerBound + (upperBound - lowerBound) * random.NextDouble();
                    }
                }
            }

            /*string result = string.Join(" ", mbest);
            string formattedResult = string.Join(" ", mbest.Select(x => x.ToString("F10")));
            string fileName = $"Rastrigin_n{n}_T{T}_dim{dim}Man.txt";
            string fullPath = Path.Combine(basePath, fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            File.AppendAllText(fullPath, formattedResult + Environment.NewLine);

            string resultf = string.Join(" ", fbest);
            string formattedResultf = string.Join(" ", fbest.Select(x => x.ToString("F10")));
            string fileNamef = $"Rastrigin_n{n}_T{T}_dim{dim}Fam.txt";
            string fullPathf = Path.Combine(basePath, fileNamef);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPathf));
            File.AppendAllText(fullPathf, formattedResultf + Environment.NewLine);*/

            t++;
        }
        double[] bestSolution;
        double bestFitness = 0;
        if (mbest_fitness < fbest_fitness)
        {
             bestSolution = mbest;
        }
        else
        {
            bestSolution = fbest;
        }


        return (bestFitness, bestSolution);
    }

}