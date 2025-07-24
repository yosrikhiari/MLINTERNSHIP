using System;
using System.IO;
using MLINTERNSHIP;

class Program
{
    static void Main()
    {
        try
        {
            using var stream = new FileStream("path/to/csv", FileMode.Open);
            var data = ImprovedDemandForecaster.LoadCsvData(stream);
            Console.WriteLine($"Loaded {data.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}