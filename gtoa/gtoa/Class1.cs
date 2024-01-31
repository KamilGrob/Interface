using algorytm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using main;
using System.Reflection;
//using TSFDE_fractional_boundary_condition; 

namespace GTOAnamespace
{
    public class GTOA : IOptimizationAlgorith
    {
        #region Parametry algorytmu:

        // funkcja celu
        public Func<double[], double> f = RastriginFunction.Evaluate;


        public string Names { get; set; }

        public ParamInfo[] ParamsInfo { get; set; }

        public IStateWriter writer { get; set; }

        public IStateReader reader { get; set; }

        public IGenerateTextReport stringReportGenerator { get; set; }

        public IGeneratePDFReport pdfReportGenerator { get; set; }

        public double[] XBest { get; set; }

        int NumofGen = 0;
        int NumofEval = 0;
        public double FBest { get; set; }
        public int NumberOfEvaluationFitnessFunction { get; set; }

        //public delegate double funkcjaCelu(params double[] arg);
        //private funkcjaCelu f;
        //public funkcjaCelu F
        //{
        //    set { f = value; }
        // }


        // ograniczenia dziedziny z dołu i góry
        private double[] brzegDol;
        private double[] brzegGora;
       
        // wymiar dziedziny
        private int wymiarZadania;

        // populacja (rozwiązania)
        private double[][] populacja;
        private int liczbaOsobnikow;

        // populacja po uczeniu przez nauczyciela
        private double[][] populacjaTeaching;

        // populacja po uczeniu przez uczniów w czasie wolnym
        private double[][] populacjaStudent;

        // tablica wartości funkcji celu osobników w populacji
        private double[] wartosciFunkcjiCelu;

        // tablica wartości funkcji celu osobników w populacjiTeaching
        private double[] wartosciFunkcjiCeluTeaching;

        // tablica wartości funkcji celu osobników w populacjiStudent
        private double[] wartosciFunkcjiCeluStudent;

        // liczba iteracji
        private int liczbaIteracji;

        // najlepszy znaleziony wynik
        

        public double yBest
        {
            get; private set;
        }

        // drugi najlepszy znaleziony wynik
        public double[] xSecondBest
        {
            get; private set;
        }

        public double ySecondBest
        {
            get; private set;
        }


        // trzeci najlepszy znaleziony wynik
        public double[] xThirdBest
        {
            get; private set;
        }

        public double yThirdBest
        {
            get; private set;
        }
        public string func;
        private int licznikFunkcjiCelu;

        // zmienna odpowiedzialna za genereowanie liczb pseudolosowych
        static Random random = new Random();

        // parametry algorytmu
        private double[] teacherAllocation;
        private double teachingFactor; // teaching factor - w artykule ozn. F

        #endregion



        #region Konstruktor

        private GTOA()
        {
            ParamsInfo = new ParamInfo[] {
        new ParamInfo("DimensionsO", "Number of dimensions", 1, 30),
        new ParamInfo("MaxGenerationsO", "Maximum number of generations", 1, 10000),
        new ParamInfo("liczbaOsobnikowO", "Number of employed bees", 1, 10000),
        };
        }
        private static GTOA gtoaInstance;

        // Metoda zwracająca jedyną instancję klasy
        public static GTOA GetInstance()
        {
            if (gtoaInstance == null)
            {
                gtoaInstance = new GTOA();
            }

            return gtoaInstance;
        }

        #endregion


        #region Funkcje pomocnicze

        // Generowanie w sposób losowy populacji startowej
        static string GetFunctionName(FitnessFunction fitnessFunction)
        {
            MethodInfo methodInfo = fitnessFunction.Method;
            // string methodName = methodInfo.Name;

            // Jeśli chcesz uzyskać pełną nazwę, możesz użyć poniższej linii zamiast powyższej
            string methodName = methodInfo.DeclaringType.Name;

            return methodName;
        }
        private void generujPopulacjeStartowa(StateAlgorithms state)
        {
            double[] tmp = new double[wymiarZadania];
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                for (int j = 0; j < wymiarZadania; j++)
                {
                    tmp[j] = brzegDol[j] + random.NextDouble() * (brzegGora[j] - brzegDol[j]);
                    populacja[i][j] = tmp[j];
                }
            }
            if (File.Exists($"D:\\Kod\\c#\\interfejs\\STATE\\GTOAState{gtoaInstance.wymiarZadania}_{func}.txt"))
            {
                state.LoadFromFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\GTOAState{gtoaInstance.wymiarZadania}_{func}.txt");

                for (int x = 0; x < gtoaInstance.wymiarZadania; x++)
                {
                    populacja[0][x] = state.XBest[x];
                }
                NumofGen = state.NumofGen;
                NumofEval = state.NumofEval;

            }
        }

        // Sortowanie populacji względem jakości rozwiązania
        private void sortuj()
        {
            int n = liczbaOsobnikow;
            do
            {
                for (int i = 0; i < n - 1; i++)
                {
                    if (wartosciFunkcjiCelu[i] > wartosciFunkcjiCelu[i + 1])
                    {
                        double[] temp = new double[wymiarZadania];
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            temp[k] = populacja[i][k];
                            populacja[i][k] = populacja[i + 1][k];
                            populacja[i + 1][k] = temp[k];
                        }
                        double tmp2 = wartosciFunkcjiCelu[i];
                        wartosciFunkcjiCelu[i] = wartosciFunkcjiCelu[i + 1];
                        wartosciFunkcjiCelu[i + 1] = tmp2;
                    }
                }
                n = n - 1;
            }
            while (n > 1);
        }

        // Funkcja ustala najlepszego osobnika w populacji oraz wwartość funkcji celu dla niego i zapisuje we własności XBest, yBest
        private void ustalNajlepszegoOsobnika()
        {
            int indeks = 0;
            double wartosc = wartosciFunkcjiCelu[0];
            for (int i = 1; i < liczbaOsobnikow; i++)
            {
                if (wartosciFunkcjiCelu[i] < wartosc)
                {
                    wartosc = wartosciFunkcjiCelu[i];
                    indeks = i;
                }
            }

            for (int k = 0; k < wymiarZadania; k++)
                XBest[k] = populacja[indeks][k];
            yBest = wartosc;
        }

        // Zapis stanu populacji
        private void zapiszStanPopulacji(string nazwaPliku, int nrIteracji)
        {
            string infoTekst = $"Iteracja nr: {nrIteracji}\n";
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                infoTekst += $"( ";
                for (int j = 0; j < wymiarZadania; j++)
                {
                    infoTekst += $"{populacja[i][j]} ";
                }
                infoTekst += $"), {wartosciFunkcjiCelu[i]}\n";
            }
            infoTekst += $"Liczba wywołań funkcji celu: {licznikFunkcjiCelu}\n";

            File.WriteAllText(nazwaPliku, infoTekst);
        }

        #endregion


        public void Solve(FitnessFunction f, List<List<double>> domain, params double[] parameters)
        {
            //this.f = fCelu;
            gtoaInstance.Names = "GTOA";
            gtoaInstance.f = (double[] args) => f(args);
            gtoaInstance.wymiarZadania = (int)parameters[0];
            gtoaInstance.liczbaOsobnikow = (int)parameters[2];
            gtoaInstance.liczbaIteracji = (int)parameters[1];
            gtoaInstance.brzegDol = new double[wymiarZadania];
            gtoaInstance.brzegGora = new double[wymiarZadania];
            for (int i = 0; i < wymiarZadania; i++)
            {
                gtoaInstance.brzegDol[i] = domain[0][i];
                gtoaInstance.brzegGora[i] = domain[1][i];
            }
            gtoaInstance.populacja = new double[liczbaOsobnikow][];
            for (int i = 0; i < liczbaOsobnikow; i++)
                populacja[i] = new double[wymiarZadania];
            gtoaInstance.populacjaTeaching = new double[liczbaOsobnikow][];
            for (int i = 0; i < liczbaOsobnikow; i++)
                populacjaTeaching[i] = new double[wymiarZadania];
            gtoaInstance.populacjaStudent = new double[liczbaOsobnikow][];
            for (int i = 0; i < liczbaOsobnikow; i++)
                populacjaStudent[i] = new double[wymiarZadania];
            gtoaInstance.wartosciFunkcjiCelu = new double[liczbaOsobnikow];
            gtoaInstance.wartosciFunkcjiCeluTeaching = new double[liczbaOsobnikow];
            gtoaInstance.wartosciFunkcjiCeluStudent = new double[liczbaOsobnikow];
            gtoaInstance.licznikFunkcjiCelu = 0;
            gtoaInstance.XBest = new double[wymiarZadania];
            gtoaInstance.xSecondBest = new double[wymiarZadania];
            gtoaInstance.xThirdBest = new double[wymiarZadania];
            gtoaInstance.teacherAllocation = new double[wymiarZadania];
            gtoaInstance.teachingFactor = 1.0;
            StateAlgorithms state = new StateAlgorithms();
            func = GetFunctionName(f);
            generujPopulacjeStartowa(state);
            int numerIteracji = 0;
            int idXBest = 0;
            int idxSecondBest = 1;
            int idxThirdBest = 2;
            double[] xMeanOfThreeBest = new double[wymiarZadania];
            double[] xMean = new double[wymiarZadania];
            double[] xTemp = new double[wymiarZadania];
            double yTemp;
            // zmienne na losowe liczby wykorzystywane w algorytmie
            double a, b, c, d, e, g;
            int losowyIndeks;
            double yLosowy;

            // liczymy wartości funkcji celu dla populacji
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                wartosciFunkcjiCelu[i] = f(populacja[i]);
                licznikFunkcjiCelu++;
            }
            // sortujemy populację względem jakości rozwiązań
            sortuj();

            // na start w tablicy wartosciFunkcjiCeluTeaching powinno być to samo to w wartosciFunkcjiCelu (tylko na start)
            for (int i = 0; i < liczbaOsobnikow; i++)
            {
                wartosciFunkcjiCeluTeaching[i] = wartosciFunkcjiCelu[i];
            }


            //////////////////////////////////////////////////////
            // główna pętla algorytmu
            while (numerIteracji < liczbaIteracji)
            {
                // ustalamy trzech najlepszych osobników i wartości funkcji celu dla nich,
                // zapisujemy te dane do XBest[], yBest, xSecondBest[], ySecondBest, xThirdBest[], yThirdBest
                for (int k = 0; k < wymiarZadania; k++)
                {
                    XBest[k] = populacja[idXBest][k];
                    xSecondBest[k] = populacja[idxSecondBest][k];
                    xThirdBest[k] = populacja[idxThirdBest][k];
                }
                yBest = wartosciFunkcjiCelu[idXBest];
                ySecondBest = wartosciFunkcjiCelu[idxSecondBest];
                yThirdBest = wartosciFunkcjiCelu[idxThirdBest];

                // ustalenie parametru teacherAllocation
                for (int k = 0; k < wymiarZadania; k++)
                    xMeanOfThreeBest[k] = (XBest[k] + xSecondBest[k] + xThirdBest[k]) / 3.0;
                if (yBest <= f(xMeanOfThreeBest))
                {
                    for (int k = 0; k < wymiarZadania; k++)
                        teacherAllocation[k] = XBest[k];
                    licznikFunkcjiCelu++;
                }
                else
                {
                    for (int k = 0; k < wymiarZadania; k++)
                        teacherAllocation[k] = xMeanOfThreeBest[k];
                    licznikFunkcjiCelu++;
                }

                ///////////////////////////////////////////////////////////////////////////////////
                // teacher phase i student phase: najpierw połowa najlepszych, potem reszta

                // zanim nauczyciel będzie uczył, liczymy średnią
                for (int i = 0; i < liczbaOsobnikow; i++)
                {
                    for (int k = 0; k < wymiarZadania; k++)
                        xMean[k] += populacja[i][k];
                }
                for (int k = 0; k < wymiarZadania; k++)
                    xMean[k] = xMean[k] / (double)liczbaOsobnikow;


                // połowa najlepszych
                for (int i = 0; i < liczbaOsobnikow / 2; i++)
                {
                    ///////////////////////////////////////////
                    // TEACHER PHASE
                    a = random.NextDouble();
                    b = random.NextDouble();
                    c = 1 - b;
                    // tworzymy osobnika po fazie uczenia przez nauczyciela
                    for (int k = 0; k < wymiarZadania; k++)
                        xTemp[k] = populacja[i][k] + a * (teacherAllocation[k] - teachingFactor * (b * xMean[k] + c * populacja[i][k]));

                    // jeśli uzyskamy lepszego osobnika, to zastępujemy go w populacjiTeaching
                    yTemp = f(xTemp);
                    licznikFunkcjiCelu++;

                    if (yTemp < wartosciFunkcjiCelu[i])
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            populacjaTeaching[i][k] = xTemp[k];
                        }
                        wartosciFunkcjiCeluTeaching[i] = yTemp;
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            populacjaTeaching[i][k] = populacja[i][k];
                        }
                        wartosciFunkcjiCeluTeaching[i] = wartosciFunkcjiCelu[i];
                    }

                    /////////////////////////////////////
                    // STUDENT PHASE
                    e = random.NextDouble();
                    g = random.NextDouble();

                    // losujemy jakiegoś osobnika (o indksie różnym od i)
                    losowyIndeks = random.Next(0, liczbaOsobnikow);
                    while (losowyIndeks == i)
                        losowyIndeks = random.Next(0, liczbaOsobnikow);

                    // przystępujemy do modyfikacji fazy student phase - wzór (7)
                    yLosowy = wartosciFunkcjiCeluTeaching[losowyIndeks];

                    // tworzymy osobnika po fazie samodzielnego lub od innych studentów uczenia się
                    if (wartosciFunkcjiCeluTeaching[i] < yLosowy)
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            xTemp[k] = populacjaTeaching[i][k] + e * (populacjaTeaching[i][k] - populacjaTeaching[losowyIndeks][k])
                                + g * (populacjaTeaching[i][k] - populacja[i][k]);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            xTemp[k] = populacjaTeaching[i][k] - e * (populacjaTeaching[i][k] - populacjaTeaching[losowyIndeks][k])
                                + g * (populacjaTeaching[i][k] - populacja[i][k]);
                        }
                    }

                    // wartość funkcji celu dla osobnika po student phase
                    yTemp = f(xTemp);
                    licznikFunkcjiCelu++;

                    //// jeśli dostaniemy coś lepszego niż po fazie teaching phase, to zastępujemy w głównej populacji
                    //if (wartosciFunkcjiCeluTeaching[i] < yTemp)
                    //{
                    //    for (int k = 0; k < wymiarZadania; k++)
                    //        populacja[i][k] = populacjaTeaching[i][k];
                    //    wartosciFunkcjiCelu[i] = wartosciFunkcjiCeluTeaching[i];
                    //}
                    //else
                    //{
                    //    for (int k = 0; k < wymiarZadania; k++)
                    //        populacja[i][k] = xTemp[k];
                    //    wartosciFunkcjiCelu[i] = yTemp;
                    //}

                    // jeśli dostaniemy coś lepszego niż po fazie teaching phase, to zastępujemy w populacji student
                    if (wartosciFunkcjiCeluTeaching[i] < yTemp)
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                            populacjaStudent[i][k] = populacjaTeaching[i][k];
                        wartosciFunkcjiCeluStudent[i] = wartosciFunkcjiCeluTeaching[i];
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                            populacjaStudent[i][k] = xTemp[k];
                        wartosciFunkcjiCeluStudent[i] = yTemp;
                    }
                }
                // pozostała reszta
                for (int i = liczbaOsobnikow / 2; i < liczbaOsobnikow; i++)
                {
                    ///////////////////////////////////////////
                    // TEACHER PHASE

                    d = random.NextDouble();
                    // tworzymy osobnika po fazie uczenia przez nauczyciela
                    for (int k = 0; k < wymiarZadania; k++)
                        xTemp[k] = populacja[i][k] + 2.0 * d * (teacherAllocation[k] - populacja[i][k]);

                    // jeśli uzyskamy lepszego osobnika, to zastępujemy go w populacjiTeaching
                    yTemp = f(xTemp);
                    licznikFunkcjiCelu++;

                    if (yTemp < wartosciFunkcjiCelu[i])
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            populacjaTeaching[i][k] = xTemp[k];
                        }
                        wartosciFunkcjiCeluTeaching[i] = yTemp;
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            populacjaTeaching[i][k] = populacja[i][k];
                        }
                        wartosciFunkcjiCeluTeaching[i] = wartosciFunkcjiCelu[i];
                    }



                    /////////////////////////////////////
                    // STUDENT PHASE
                    e = random.NextDouble();
                    g = random.NextDouble();

                    // losujemy jakiegoś osobnika (o indksie różnym od i)
                    losowyIndeks = random.Next(0, liczbaOsobnikow);
                    while (losowyIndeks == i)
                        losowyIndeks = random.Next(0, liczbaOsobnikow);

                    // przystępujemy do modyfikacji fazy student phase - wzór (7)
                    yLosowy = wartosciFunkcjiCeluTeaching[losowyIndeks];

                    // tworzymy osobnika po fazie samodzielnego lub od innych studentów uczenia się
                    if (wartosciFunkcjiCeluTeaching[i] < yLosowy)
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            xTemp[k] = populacjaTeaching[i][k] + e * (populacjaTeaching[i][k] - populacjaTeaching[losowyIndeks][k])
                                + g * (populacjaTeaching[i][k] - populacja[i][k]);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                        {
                            xTemp[k] = populacjaTeaching[i][k] - e * (populacjaTeaching[i][k] - populacjaTeaching[losowyIndeks][k])
                                + g * (populacjaTeaching[i][k] - populacja[i][k]);
                        }
                    }

                    // wartość funkcji celu dla osobnika po student phase
                    yTemp = f(xTemp);
                    licznikFunkcjiCelu++;

                    //// jeśli dostaniemy coś lepszego niż po fazie teaching phase, to zastępujemy w głównej populacji
                    //if (wartosciFunkcjiCeluTeaching[i] < yTemp)
                    //{
                    //    for (int k = 0; k < wymiarZadania; k++)
                    //        populacja[i][k] = populacjaTeaching[i][k];
                    //    wartosciFunkcjiCelu[i] = wartosciFunkcjiCeluTeaching[i];
                    //}
                    //else
                    //{
                    //    for (int k = 0; k < wymiarZadania; k++)
                    //        populacja[i][k] = xTemp[k];
                    //    wartosciFunkcjiCelu[i] = yTemp;
                    //}

                    // jeśli dostaniemy coś lepszego niż po fazie teaching phase, to zastępujemy w populacji student
                    if (wartosciFunkcjiCeluTeaching[i] < yTemp)
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                            populacjaStudent[i][k] = populacjaTeaching[i][k];
                        wartosciFunkcjiCeluStudent[i] = wartosciFunkcjiCeluTeaching[i];
                    }
                    else
                    {
                        for (int k = 0; k < wymiarZadania; k++)
                            populacjaStudent[i][k] = xTemp[k];
                        wartosciFunkcjiCeluStudent[i] = yTemp;
                    }
                }

                // zerujemy średnią
                for (int k = 0; k < wymiarZadania; k++)
                    xMean[k] = 0;

                // do następnej populacji głównej przepisujemy osobniki po fazie student phase
                for (int i = 0; i < liczbaOsobnikow; i++)
                {
                    for (int k = 0; k < wymiarZadania; k++)
                    {
                        populacja[i][k] = populacjaStudent[i][k];
                    }
                    wartosciFunkcjiCelu[i] = wartosciFunkcjiCeluStudent[i];
                }

                // sortujemy populację
                sortuj();
                Directory.CreateDirectory($@"uczniowie={liczbaOsobnikow} iteracje={liczbaIteracji}");
                zapiszStanPopulacji($@"uczniowie={liczbaOsobnikow} iteracje={liczbaIteracji}/GTOA iteracja={numerIteracji}.txt",
                    numerIteracji);
                state.setBest(XBest, yBest, licznikFunkcjiCelu + NumofEval, numerIteracji + 1 + NumofGen);
                Console.Write($"numer iteracj : {NumofGen} + {numerIteracji}");
                state.SaveToFileStateOfAlgorithm($"D:\\Kod\\c#\\interfejs\\STATE\\GTOAState{gtoaInstance.wymiarZadania}_{func}.txt");
                numerIteracji++;
            }

            Console.Write($"XBest: ");
            for (int k = 0; k < wymiarZadania; k++)
                Console.Write($"{XBest[k]} ");
            Console.Write($"fcelu: {yBest}");
            FBest = yBest;
            Console.Write("\n");
        }
    }
}
