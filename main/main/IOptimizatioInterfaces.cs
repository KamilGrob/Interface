public interface IStateWriter
{
    void SaveToFileStateOfAlgorithm(string path);
}

public interface IStateReader
{
    void LoadFromFileStateOfAlgorithm(string path);
}

public interface IGeneratePDFReport
{
    void GenerateReport(string path);
}

public interface IGenerateTextReport
{
    string ReportString { get; }
}



