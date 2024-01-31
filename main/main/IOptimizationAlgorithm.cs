namespace algorytm
{
    public delegate double FitnessFunction(params double[] arg);
    public interface IOptimizationAlgorith
    {
        string Names { get; set; }

        void Solve(FitnessFunction f, List<List<double>> domainList, params double[] parameters);

        ParamInfo[] ParamsInfo { get; set; }

        IStateWriter writer { get; set; }

        IStateReader reader { get; set; }

        IGenerateTextReport stringReportGenerator { get; set; }

        IGeneratePDFReport pdfReportGenerator { get; set; }

        double[] XBest { get; set; }

        double FBest { get; set; }

        int NumberOfEvaluationFitnessFunction { get; set; }
    }
}