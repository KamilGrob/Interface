public class OptimizationParameters
{
    public double LowerBound { get; set; }
    public double UpperBound { get; set; }
    public int Dimensions { get; set; }
    public int MaxGenerations { get; set; }
    public int TotalBees { get; set; }
    public List<List<double>> domain { get; set; }

    public string[] ObjectiveFunction { get; set; }

    public string[] SelectedAlgorithms { get; set; }
    public double[] Parameters { get; set; }
}

public class AlgorithmResult
{
    public string Algorithm { get; set; }
    public double Fitness { get; set; }
    public double[] Solution { get; set; }
}

public class ParamInfo
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public double UpperBoundary { get; set; }
    public double LowerBoundary { get; set; }

    public ParamInfo(string name, string description, double upperBoundary, double lowerBoundary)
    {
        Name = name;
        Description = description;
        UpperBoundary = upperBoundary;
        LowerBoundary = lowerBoundary;
    }
}

