using algorytm;
using Gradient;
using GTOAnamespace;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TSFDE_fractional_boundary_condition;
using ZadanieAA;
using static iText.IO.Image.Jpeg2000ImageData;


    public class NewPageController : Controller
    {
        public IActionResult Index()
        {
            // Utwórz instancję algorytmu ABC
            var abcAlgorithm = ABC.GetInstance();
            // Pobierz ParamsInfo z ABC
            var paramsInfoList = new List<ParamInfo[]> { abcAlgorithm.ParamsInfo };

            // Dodaj informacje o parametrach z innych algorytmów (dla przykładu Gradient)
            var gradientAlgorithm = GradientDescent.GetInstance(); // Zakładając, że masz odpowiednią implementację dla GradientDescent
            paramsInfoList.Add(gradientAlgorithm.ParamsInfo);

            var gotaAlgorithm = GTOA.GetInstance();
            paramsInfoList.Add(gotaAlgorithm.ParamsInfo);
            // Możesz dodać informacje o parametrach z innych algorytmów, jeśli są dostępne

            // Przekaż listę z danymi ParamsInfo do widoku
            ViewBag.ParamsInfoList = paramsInfoList;

            return View();
        }
    public IActionResult SomeAction()
    {
        // Przekierowanie na nową stronę
        return RedirectToAction("Index", "Settings");
    }
    public IActionResult SomeActionGTOA()
    {
        // Przekierowanie na nową stronę
        return RedirectToAction("Index", "SettingsGTOA");
    }
    public IActionResult SomeActionGradient()
    {
        // Przekierowanie na nową stronę
        return RedirectToAction("Index", "SettingsGradient");
    }
    [HttpPost]
        public JsonResult RunOptimization([FromBody] OptimizationParameters optimizationParameters)
        {
            // Dostosuj logikę optymalizacji używając parametrów z modelu
            TSFDE_fractional_boundary tsfde_inv = new TSFDE_fractional_boundary();
        ObjectiveFunction of = new ObjectiveFunction();

        //Console.Write($"dane parametru {optimizationParameters.SelectedAlgorithms}");
        // Ustaw funkcję celu na podstawie wartości przekazanej z formularza
        FitnessFunction fitnessFunctionDelegate = null;
            string funcName = null;

            var selectedFunction = optimizationParameters.ObjectiveFunction;
            //Console.Write($"jebane funkcje sele{selectedFunction[1]}");
            var results = new List<AlgorithmResult>();
   
        //Console.WriteLine(function);
        switch (selectedFunction[0])
            {
                case "SphereFunction":
                    fitnessFunctionDelegate = SphereFunction.Evaluate;
                    funcName = "SphereFunction";
                    break;
                case "RosenbrockFunction":
                    fitnessFunctionDelegate = RosenbrockFunction.Evaluate;
                    funcName = "RosenbrockFunction";
                    break;
                case "RastriginFunction":
                    fitnessFunctionDelegate = RastriginFunction.Evaluate;
                    funcName = "RastriginFunction";
                    break;
                case "tsfde_inv":
                    fitnessFunctionDelegate = tsfde_inv.fintnessFunction;
                    funcName = "tsfde_inv";
                optimizationParameters.Parameters[0] = 7;
                break;
                case "przyklad":
                    fitnessFunctionDelegate = of.FunkcjaCelu.Wartosc;
                    funcName = "przyklad";
                break;
            default:
                    fitnessFunctionDelegate = RastriginFunction.Evaluate;
                    funcName = "RastriginFunction";
                    break;
            }
            var selectedAlgorithms = optimizationParameters.SelectedAlgorithms; // Pobierz zaznaczone algorytmy





            foreach (var algorithm in selectedAlgorithms)
            {
                IOptimizationAlgorith optimizationAlgorithm = null;
            Console.WriteLine(algorithm);
                switch (algorithm)
                {
                    case "ABC":
                        optimizationAlgorithm = ABC.GetInstance();
                        using(StreamReader reader = new StreamReader("D:\\Kod\\c#\\interfejs\\settings\\abc.txt"))
                        {
                            // Wczytaj dane z pliku, podziel je na elementy i przekonwertuj na liczby
                            double[] FileNumbers = reader.ReadLine()
                                .Split(',')
                                 .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                                .ToArray();

                            // Dołącz dane z pliku do tablicy parameters
                            optimizationParameters.Parameters = optimizationParameters.Parameters.Concat(FileNumbers).ToArray();
                       

                    }
                        break;
                    case "Gradient":
                        optimizationAlgorithm = GradientDescent.GetInstance();
                        using (StreamReader reader = new StreamReader("D:\\Kod\\c#\\interfejs\\settings\\gradient.txt"))
                        {
                            // Wczytaj dane z pliku, podziel je na elementy i przekonwertuj na liczby
                            double[] FileNumbers = reader.ReadLine()
                                .Split(',')
                                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                                .ToArray();

                            // Dołącz dane z pliku do tablicy parameters
                            optimizationParameters.Parameters = optimizationParameters.Parameters.Concat(FileNumbers).ToArray();
                        }
                        break;
                    case "GTOA":
                        optimizationAlgorithm = GTOA.GetInstance();
                        using (StreamReader reader = new StreamReader("D:\\Kod\\c#\\interfejs\\settings\\gtoa.txt"))
                        {
                            // Wczytaj dane z pliku, podziel je na elementy i przekonwertuj na liczby
                            double[] FileNumbers = reader.ReadLine()
                                .Split(',')
                                .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                                .ToArray();

                            // Dołącz dane z pliku do tablicy parameters
                            optimizationParameters.Parameters = optimizationParameters.Parameters.Concat(FileNumbers).ToArray();
                        }
                        break;

                    default:

                        break;
                }
                if (optimizationAlgorithm != null)
                {

                    var reportGenerator = new ReportGenerator(optimizationAlgorithm);
                    DateTime currentDate = DateTime.Now;
                    string timestamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
                    // Inicjalizacja domain jako nowej instancji listy list
                    optimizationParameters.domain = new List<List<double>>();

                    // Dodawanie dwóch pustych list do domain (jeśli wymaga tego struktura)
                    optimizationParameters.domain.Add(new List<double>());
                    optimizationParameters.domain.Add(new List<double>());
                    if (fitnessFunctionDelegate == SphereFunction.Evaluate)
                    {
                        for (int i = 0; i < optimizationParameters.Parameters[0]; i++)
                        {
                            optimizationParameters.domain[0].Add(-10);
                            optimizationParameters.domain[1].Add(10);
                        }
                    }
                    if (fitnessFunctionDelegate == RosenbrockFunction.Evaluate)
                    {
                        for (int i = 0; i < optimizationParameters.Parameters[0]; i++)
                        {
                            optimizationParameters.domain[0].Add(-10);
                            optimizationParameters.domain[1].Add(10);
                        }
                    }
                    if (fitnessFunctionDelegate == RastriginFunction.Evaluate)
                    {
                        for (int i = 0; i < optimizationParameters.Parameters[0]; i++)
                        {
                            optimizationParameters.domain[0].Add(-5.12);
                            optimizationParameters.domain[1].Add(5.12);
                        }
                    }
                    if (fitnessFunctionDelegate == tsfde_inv.fintnessFunction)
                    {
                        double[] a = { 0.1, 1.1, 1.0, -70.0, 250.0, -30.0, 50.0 };
                        double[] b = { 0.9, 1.9, 5.0, -20.0, 450.0, -10.0, 250.0 };

                        optimizationParameters.domain[0].AddRange(a);
                        optimizationParameters.domain[1].AddRange(b);
                    }
                if (fitnessFunctionDelegate == of.FunkcjaCelu.Wartosc)
                {
                    for (int i = 0; i < optimizationParameters.Parameters[0]; i++)
                    {
                        optimizationParameters.domain[0].Add(0.5);
                        optimizationParameters.domain[1].Add(1.5);
                    }
                }
                optimizationAlgorithm.Solve(fitnessFunctionDelegate, optimizationParameters.domain, optimizationParameters.Parameters);
                    reportGenerator.SetParameters(optimizationParameters.Parameters, funcName);
                    reportGenerator.GenerateReport($"D:\\Kod\\c#\\interfejs\\RAPORTY\\raport_{timestamp}.pdf");

                    results.Add(new AlgorithmResult
                    {

                        Algorithm = algorithm,
                        Fitness = optimizationAlgorithm.FBest,
                        Solution = optimizationAlgorithm.XBest
                    });
                }
            }


            return Json(results);
        }
        
    }
