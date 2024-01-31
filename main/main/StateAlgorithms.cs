using algorytm;

namespace main
{
    public class StateAlgorithms : IStateWriter, IStateReader
    {

        public double[] XBest;
        public double FBest;
        public int NumofEval;
        public int NumofGen;


       
        public void setBest(double[] XBest, double Fbest, int NumofEval, int NumofGen)
        {
            this.XBest = XBest;
            this.FBest = Fbest;
            this.NumofEval = NumofEval;
            this.NumofGen = NumofGen;
        }

        public void SaveToFileStateOfAlgorithm(string path)
        {
            using (StreamWriter writer = new StreamWriter(path, true))
            {

                // Zapisz populację wraz z wartością funkcji dopasowania

                writer.WriteLine($"{NumofGen} {NumofEval} {string.Join(" ", XBest)} {FBest}");


            }
        }
        public void LoadFromFileStateOfAlgorithm(string path) 
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string line;

                // Wczytaj ostatnią linię z pliku
                while ((line = reader.ReadLine()) != null)
                {
                    // Pomijamy puste linie
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Wczytaj dane z linii
                        string[] parts = line.Split(' ');
                        NumofGen = int.Parse(parts[0]);
                        NumofEval = int.Parse(parts[1]);
                        XBest = Array.ConvertAll(parts.Skip(2).Take(parts.Length - 3).ToArray(), double.Parse);
                        FBest = double.Parse(parts.Last());
                    }
                }
            }
        }
    }
}
