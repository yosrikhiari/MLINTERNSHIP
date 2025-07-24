using System;
using System.IO;
using MLINTERNSHIP;

class Program
{
    static void Main()
    {
        try
        {
            using var stream = new FileStream(@"C:\Users\yosri\Desktop\projects for me\intership 4éme\MLINTERNSHIP\supply_chain_data.csv", FileMode.Open);
            var data = ImprovedDemandForecaster.LoadCsvData(stream);
            Console.WriteLine($"Loaded {data.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}