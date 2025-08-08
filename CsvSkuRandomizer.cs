using System;
using System.IO;

public static class CsvSkuRandomizer
{
    public static void RandomizeSkuNumbers(string inputPath, string outputPath)
    {
        var random = new Random();
        var lines = File.ReadAllLines(inputPath);

        using (var writer = new StreamWriter(outputPath))
        {
            // Write header
            writer.WriteLine(lines[0]);

            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');

                // Find SKU column and randomize its number
                if (columns.Length > 1 && columns[1].StartsWith("SKU"))
                {
                    int randomSku = random.Next(1, 11); // 1 to 10 inclusive
                    columns[1] = $"SKU{randomSku}";
                }

                writer.WriteLine(string.Join(",", columns));
            }
        }
    }
}

 CsvSkuRandomizer.RandomizeSkuNumbers("supply_chain_data.csv", "supply_chain_data_randomized.csv");

