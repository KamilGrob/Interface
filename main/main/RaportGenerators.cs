using System.Reflection.Metadata;
using System.Text;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

using System.IO;
using algorytm;



public class ReportGenerator : IGeneratePDFReport, IGenerateTextReport
{
    private IOptimizationAlgorith optimizationAlgorit;
    private double[] parameters;
    private string func;

    public ReportGenerator(IOptimizationAlgorith optimizationAlgorithm)
    {
        this.optimizationAlgorit = optimizationAlgorithm;
    }
    public void SetParameters(double []parameters, string func)
    {
        this.parameters = parameters;
        this.func = func;
    }
    
    public void GenerateReport(string path) {
        //public string ReportString { get { return "Tekst raportu"; } }
        // Implementacja interfejsu IGeneratePDFReport
        /*private IOptimizationAlgorith optimizationAlgorit;

        public ReportGenerator(IOptimizationAlgorith optimizationAlgorithm)
        {
            this.optimizationAlgorit = optimizationAlgorithm;
        }
        public void GenerateReport(string path)
        {
            using (PdfWriter writer = new PdfWriter(path))
            {
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    using (iText.Layout.Document document = new iText.Layout.Document(pdf))
                    {
                        // var document = new iText.Layout.Document(pdf);

                        // Dodaj treść raportu
                        document.Add(new Paragraph("Raport PDF"));

                        // Dodaj informacje o najlepszym osobniku, wartości funkcji celu, liczbie wywołań itp.
                        document.Add(new Paragraph($"Najlepszy osobnik: {optimizationAlgorit.XBest}"));
                        document.Add(new Paragraph($"Wartość funkcji celu: {optimizationAlgorit.FBest}"));
                        document.Add(new Paragraph($"Liczba wywołań funkcji celu: {optimizationAlgorit.NumberOfEvaluationFitnessFunction}"));

                        // Dodaj parametry algorytmu
                        foreach (var paramInfo in optimizationAlgorit.ParamsInfo)
                        {
                            document.Add(new Paragraph($"{paramInfo.Name}: 1"));
                        }
                    }
                }
            }*/

        iTextSharp.text.Document doc = new iTextSharp.text.Document();
        FileStream fs = new FileStream(path, FileMode.Create);
        PdfWriter writer = PdfWriter.GetInstance(doc, fs);
        doc.Open();
        doc.Add(new Paragraph("Raport PDF"));
        doc.Add(new Paragraph($"Algorytm optymalizacyjny: {optimizationAlgorit.Names}"));
        doc.Add(new Paragraph($"Funkcja celu: {func}"));
        doc.Add(new Paragraph($"Najlepszy osobnik: {string.Join(", ", optimizationAlgorit.XBest)}"));
        doc.Add(new Paragraph($"Wartość funkcji celu: {optimizationAlgorit.FBest}"));
        doc.Add(new Paragraph($"Liczba wywołań funkcji celu: {optimizationAlgorit.NumberOfEvaluationFitnessFunction}"));

        // Dodaj parametry algorytmu
        int i = 0;
        foreach (var paramInfo in optimizationAlgorit.ParamsInfo)
        {
            doc.Add(new Paragraph($"{paramInfo.Name}: {parameters[i]}"));
            i++;
        }
        doc.Close();
        fs.Close(); }
        ///}

        // Implementacja interfejsu IGenerateTextReport
        public string ReportString
        {
            get
            {
                // Implementacja generowania raportu w postaci tekstu
                return "Tekst raportu";
            }
        }
    }
