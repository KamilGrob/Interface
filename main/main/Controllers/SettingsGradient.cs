using Microsoft.AspNetCore.Mvc;

public class SettingsGradient : Controller
{
    public IActionResult Index()
    {
        string filePath = "D:\\Kod\\c#\\interfejs\\settings\\gradient.txt";

        var model = ReadSettingsFile(filePath);

        return View(model);
    }

    private SettingsModel ReadSettingsFile(string filePath)
    {
        var lines = System.IO.File.ReadAllLines(filePath);
        var values = lines[0].Split(',').Select(double.Parse).ToList();

        return new SettingsModel
        {
            FileName = System.IO.Path.GetFileNameWithoutExtension(filePath),
            Values = values
        };
    }

    [HttpPost]
    public IActionResult Index(SettingsModel model)
    {
        try
        {
            // Zapisz nowe wartości do pliku
            SaveSettingsToFile(model);
        }
        catch (Exception ex)
        {
            // Obsługa błędów zapisu
            ModelState.AddModelError(string.Empty, $"Error saving settings: {ex.Message}");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    private void SaveSettingsToFile(SettingsModel model)
    {
        // Ścieżka do pliku
        string filePath = $"ścieżka/do/folderu/settings/{model.FileName}.txt";

        // Zapisz nowe wartości do pliku
        System.IO.File.WriteAllText(filePath, string.Join(",", model.Values));
    }

}
