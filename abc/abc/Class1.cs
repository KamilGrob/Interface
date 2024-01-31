using System;
using System.IO;
using System.Reflection;
using System.Reflection.PortableExecutable;
using algorytm;
using main;
//using TSFDE_fractional_boundary_condition;

public class ABC : IOptimizationAlgorith
{
    
    UtilityFunctions utilityFunctions = UtilityFunctions.Instance;
    public Random random = new Random();

    public int employedBees = 20;
    public int onlookerBees = 20;
    public int totalBees = 40;
    public int maxGenerations = 100;

    public int dimensions = 3;
    public double[] lowerBound;
    public double[] upperBound;

    public double limit = 0.6;
    //TSFDE_fractional_boundary tsfde_inv = new TSFDE_fractional_boundary();

    public Func<double[], double> objectiveFunction = RastriginFunction.Evaluate;

    public string Names { get; set; }

    public ParamInfo[] ParamsInfo { get; set; }

    public IStateWriter writer { get; set; }

    public IStateReader reader { get; set; }

    public IGenerateTextReport stringReportGenerator { get; set; }

    public IGeneratePDFReport pdfReportGenerator { get; set; }

    public double[] XBest { get; set; }

    public double FBest { get; set; }

    public int NumberOfEvaluationFitnessFunction { get; set; }
    public string func;

    private ABC()
    {
        ParamsInfo = new ParamInfo[] {
        new ParamInfo("Dimensions", "Number of dimensions", 1, 30),
        new ParamInfo("MaxGenerations", "Maximum number of generations", 1, 10000),
        new ParamInfo("employedBees", "Number of employed bees", 1, 10000),
        new ParamInfo("onlookerBees", "Number of onlooker bees", 1, 10000),
        new ParamInfo("limit", "Limit parameter", 0.1, 1)
        };
    }
    

    private static ABC abcInstance;

    // Metoda zwracająca jedyną instancję klasy
    public static ABC GetInstance()
    {
        if (abcInstance == null)
        {
            abcInstance = new ABC();
        }

        return abcInstance;
    }
    static string GetFunctionName(FitnessFunction fitnessFunction)
    {
        MethodInfo methodInfo = fitnessFunction.Method;
       // string methodName = methodInfo.Name;

        // Jeśli chcesz uzyskać pełną nazwę, możesz użyć poniższej linii zamiast powyższej
        string methodName = methodInfo.DeclaringType.Name;

        return methodName;
    }
    public void Solve(FitnessFunction f, List<List<double>> domain, params double[] parameters)
    {
        
         
        // Upewnij się, że istnieje instancja ABC
        if (abcInstance == null)
        {
            Console.WriteLine("Nie istnieje instancja ABC");
            abcInstance = new ABC();
        }
        StateAlgorithms state = new StateAlgorithms();
        // Wczytaj stan z reader, jeśli został ustawiony
        
        // Ustaw parametry algorytmu

        abcInstance.Names = "ABC";
        // Ustaw parametry algorytmu

        //f = tsfde_inv.fintnessFunction;

        abcInstance.dimensions = (int)parameters[0];
        abcInstance.maxGenerations = (int)parameters[1];
        abcInstance.employedBees = (int)parameters[2];
        abcInstance.onlookerBees = (int)parameters[3];
        abcInstance.limit = (int)parameters[4];
        abcInstance.totalBees = abcInstance.onlookerBees + abcInstance.employedBees;
        abcInstance.lowerBound = new double[dimensions];
        abcInstance.upperBound = new double[dimensions];
        
          for (int i = 0; i < dimensions; i++)
            {
                abcInstance.lowerBound[i] = domain[0][i];
                abcInstance.upperBound[i] = domain[1][i];
            }

         func = GetFunctionName(f);
        //Console.Write(func);

        // Ustaw funkcję celu
        abcInstance.objectiveFunction = (double[] args) => f(args);
        abcInstance.writer = writer;

        // Wywołaj funkcję RunOptimization
        RunOptimization(state);

        abcInstance.NumberOfEvaluationFitnessFunction = FunCounter.getCounter();
        FunCounter.reset();
        XBest = abcInstance.XBest;
        FBest = abcInstance.FBest;

    }

    public void RunOptimization(StateAlgorithms state)
    {
        double[][] solutions = utilityFunctions.InitializeSolutions(abcInstance);
        int NumofGen =0;
        int NumofEval=0;
        if (File.Exists($"D:\\Kod\\c#\\interfejs\\STATE\\ABCState{abcInstance.dimensions}_{func}.txt"))
        {
            state.LoadFromFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\ABCState{abcInstance.dimensions}_{func}.txt");

            for (int x = 0; x < abcInstance.dimensions; x++)
            {
                solutions[0][x] = state.XBest[x];
            }
            NumofGen = state.NumofGen;
            NumofEval = state.NumofEval;

        }


        
        double[] fitness = utilityFunctions.EvaluateSolutions(solutions, abcInstance.objectiveFunction, abcInstance);
        int bestIndex;
        double bestFitness;
        double[] bestSolution;
      //  StateAlgorithms state = new StateAlgorithms();

        for (int generation = 0; generation < abcInstance.maxGenerations; generation++)
        {
            bestIndex = utilityFunctions.GetBestSolutionIndex(fitness);
            bestFitness = fitness[bestIndex];
            bestSolution = solutions[bestIndex];
            FBest = bestFitness;
            XBest = bestSolution;
            state.setBest(XBest, FBest, FunCounter.getCounter()+ NumofEval, generation + 1 + NumofGen);
            state.SaveToFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\ABCState{abcInstance.dimensions}_{func}.txt");
            EmployedBeePhase(solutions, fitness, abcInstance.objectiveFunction);
            OnlookerBeePhase(solutions, fitness, abcInstance.objectiveFunction);
            //ScoutBeesPhase(solutions, fitness, abcInstance.objectiveFunction);
            if (abcInstance.writer != null)
            {
                abcInstance.writer.SaveToFileStateOfAlgorithm("path/to/state/file.txt");
            }
            
        }

        bestIndex = utilityFunctions.GetBestSolutionIndex(fitness);
        bestFitness = fitness[bestIndex];
        bestSolution = solutions[bestIndex];

        FBest = bestFitness;
        XBest = bestSolution;
        
        //return (bestFitness, bestSolution);

    }
    public  void EmployedBeePhase(double[][] solutions, double[] fitness, Func<double[], double> objectiveFunction)
    {
        for (int i = 0; i < employedBees; i++)
        {
            int neighborIndex = utilityFunctions.GetRandomNeighbor(i, employedBees, abcInstance);
            int dimensionToChange = random.Next(dimensions);
            double[] newSolution = new double[dimensions];

            for (int j = 0; j < dimensions; j++)
            {
                newSolution[j] = solutions[i][j];
            }
            newSolution[dimensionToChange] += (random.NextDouble() - 0.5) * 2 * limit * (solutions[i][dimensionToChange] - solutions[neighborIndex][dimensionToChange]);
            for (int j = 0; j < newSolution.Length; ++j)
            {
                newSolution[j] = utilityFunctions.Clamp(newSolution[j], lowerBound[j], upperBound[j]);
            }
            double newFitness = objectiveFunction(newSolution);

            if (newFitness < fitness[i])
            {
                solutions[i][dimensionToChange] = newSolution[dimensionToChange];
                fitness[i] = newFitness;
            }
            
            
        }
       // Console.WriteLine("EmplyedBee");
        //Console.WriteLine($"solutions: {solutions[1][0]}, {solutions[1][1]}");
       // Console.WriteLine($"fitness: {fitness[1]}");
    }

    public  void OnlookerBeePhase(double[][] solutions, double[] fitness, Func<double[], double> objectiveFunction)
    {
        double totalFitness = fitness.Sum();
        double[] probabilities = fitness.Select(f => f / totalFitness).ToArray();

        for (int i = 0; i < onlookerBees; i++)
        {
            int selectedBee = utilityFunctions.SelectBee(probabilities, abcInstance);
            int neighborIndex = utilityFunctions.GetRandomNeighbor(selectedBee, employedBees, abcInstance);
            int dimensionToChange = random.Next(dimensions);

            double[] newSolution = new double[dimensions];
  
            for (int j = 0; j < dimensions; j++)
            {
                
                newSolution[j] = solutions[selectedBee][j];
            }

            newSolution[dimensionToChange] += (random.NextDouble() - 0.5) * 2 * limit * (solutions[selectedBee][dimensionToChange] - solutions[neighborIndex][dimensionToChange]);
            for (int j = 0; j < newSolution.Length; ++j)
            {
                newSolution[j] = utilityFunctions.Clamp(newSolution[j], lowerBound[j], upperBound[j]);
            }
            double newFitness = objectiveFunction(newSolution);

            if (newFitness < fitness[selectedBee])
            {
                solutions[selectedBee][dimensionToChange] = newSolution[dimensionToChange];
                fitness[selectedBee] = newFitness;
            }

        }
       // Console.WriteLine("OnlookerBee");
       // Console.WriteLine($"solutions: {solutions[1][0]}, {solutions[1][1]}");
       // Console.WriteLine($"fitness: {fitness[1]}");
    }

    public void ScoutBeesPhase(double[][] solutions, double[] fitness, Func<double[], double> objectiveFunction)
    {
        for (int i = 0; i < employedBees + onlookerBees; i++)
        {
            if (random.NextDouble() < 0.01) // 1% chance of becoming a scout
            {
                double[] newSolution = new double[dimensions];

                for (int j = 0; j < dimensions; j++)
                {
                    newSolution[j] = random.NextDouble() * 10;
                }
                fitness[i] = objectiveFunction(newSolution);

                for (int j = 0; j < dimensions; j++)
                {
                    solutions[i][j] = newSolution[j];
                }
            }
        }
    }
}






