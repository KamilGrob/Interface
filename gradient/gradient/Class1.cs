using System;
using System.Reflection;
using algorytm;
using main;


namespace Gradient
{
    public class GradientDescent: IOptimizationAlgorith
    {
        public double learningRatee = 0.01;
        public Func<double[], double> objectiveFunction = RastriginFunction.Evaluate;
        public int maxGenerationss = 100;

        public int dimensionss = 3;
        public double[] lowerBoundd;
        public double[] upperBoundd;

        public string Names { get; set; }

         public ParamInfo[] ParamsInfo { get; set; }

        public IStateWriter writer { get; set; }

        public IStateReader reader { get; set; }

        public IGenerateTextReport stringReportGenerator { get; set; }

        public IGeneratePDFReport pdfReportGenerator { get; set; }

         public double[] XBest { get; set; }

         public double FBest { get; set; }
        public string func;
        public int NumberOfEvaluationFitnessFunction { get; set; }
        // Singleton




        // Prywatny konstruktor zapobiegający utworzeniu więcej niż jednej instancji
         private GradientDescent() {

             ParamsInfo = new ParamInfo[] {
             new ParamInfo("DimensionsG", "Number of dimensions", 1, 30),
             new ParamInfo("MaxGenerationsG", "Maximum number of generations", 1, 10000),
             new ParamInfo("learningRateG", "Number of gradient multiplier",  0.00000001, 1),
             };
         }

        private static GradientDescent gradientInstance;

        // Metoda zwracająca jedyną instancję klasy
        public static GradientDescent GetInstance()
        {
            if (gradientInstance == null)
            {
                gradientInstance = new GradientDescent();
            }

            return gradientInstance;
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
             if (gradientInstance == null)
             {
                Console.WriteLine("Nie istnieje instancja gradient");
                 gradientInstance = new GradientDescent();
            }
            gradientInstance.Names = "Gradient";


             gradientInstance.dimensionss = (int)parameters[0];
            gradientInstance.maxGenerationss = (int)parameters[1];
            gradientInstance.learningRatee = parameters[2];
            gradientInstance.lowerBoundd = new double[dimensionss];
            gradientInstance.upperBoundd = new double[dimensionss];
            for (int i = 0; i < dimensionss; i++)
            {
                gradientInstance.lowerBoundd[i] = domain[0][i];
                gradientInstance.upperBoundd[i] = domain[1][i];
            }
            func = GetFunctionName(f);

            // Ustaw funkcję celu
            gradientInstance.objectiveFunction = (double[] args) => f(args);

            gradientInstance.writer = writer;



             //Wywołaj funkcję RunOptimization
             RunOptimization();
            gradientInstance.NumberOfEvaluationFitnessFunction = FunCounter.getCounter();
            FunCounter.reset();

            XBest = gradientInstance.XBest;
             FBest = gradientInstance.FBest;

        }


        public void RunOptimization()
        {
            StateAlgorithms state = new StateAlgorithms();
            double[] initialGuess = UtilityFunctions.GetRandomInitialGuess(dimensionss, lowerBoundd, upperBoundd);
            
            RunGradientDescent(initialGuess, out double[] bestSolution, out double bestObjectiveValue, state);


            //return (bestObjectiveValue, bestSolution);
            FBest = bestObjectiveValue;
            XBest = bestSolution;

            if (gradientInstance.writer != null)
            {
                gradientInstance.writer.SaveToFileStateOfAlgorithm("path/to/state/final_state.txt");
            }
             }

         void RunGradientDescent(double[] initialGuess, out double[] bestSolution, out double bestObjectiveValue, StateAlgorithms state)
            {

            int NumofGen = 0;
            int NumofEval = 0;
            if (File.Exists($"D:\\Kod\\c#\\interfejs\\STATE\\GradientState{gradientInstance.dimensionss}_{func}.txt"))
            {
                state.LoadFromFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\GradientState{gradientInstance.dimensionss}_{func}.txt");

                for (int x = 0; x < gradientInstance.dimensionss; x++)
                {
                    initialGuess[x] = state.XBest[x];
                }
                NumofGen = state.NumofGen;
                NumofEval = state.NumofEval;

            }
            double[] currentSolution = initialGuess;
             bestSolution = (double[])currentSolution.Clone();
             bestObjectiveValue = gradientInstance.objectiveFunction(currentSolution);

             for (int iteration = 0; iteration < gradientInstance.maxGenerationss; iteration++)
             {
                FBest = bestObjectiveValue;
                XBest = bestSolution;
                state.setBest(XBest, FBest, FunCounter.getCounter()+ NumofEval, iteration + 1+NumofGen);
                state.SaveToFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\GradientState{gradientInstance.dimensionss}_{func}.txt");

                double[] gradient = ComputeGradient(currentSolution);

                 // Aktualizacja rozwiązania zgodnie z regułą gradientową
                 for (int i = 0; i < currentSolution.Length; i++)
                 {
                     currentSolution[i] -= gradientInstance.learningRatee * gradient[i];
                }
                
                 double currentObjectiveValue = gradientInstance.objectiveFunction(currentSolution);

                // Aktualizacja najlepszego rozwiązania
                 if (currentObjectiveValue < bestObjectiveValue)
                 {
                     bestObjectiveValue = currentObjectiveValue;
                     bestSolution = (double[])currentSolution.Clone();
                 }

             }
        }

        static double[] ComputeGradient(double[] point)
        {
            double[] gradient = new double[point.Length];

            for (int i = 0; i < point.Length; i++)
                {
                    gradient[i] = 2 * (point[i] + 10 * Math.PI * Math.Sin(2 * Math.PI * point[i]));
                }

            return gradient;
        }
    }
}