using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using XGBoostSharp;


namespace MLINTERNSHIP
{
    public class SupplyChainData
    {
        [Name("Date")]
        public DateTime Date { get; set; }
        [Name("Product type")]
        public string ProductType { get; set; } = string.Empty;
        [Name("SKU")]
        public string SKU { get; set; } = string.Empty;
        [Name("Price")]
        public double Price { get; set; }
        [Name("Availability")]
        public int Availability { get; set; }
        [Name("Number of products sold")]
        public int NumberOfProductsSold { get; set; }
        [Name("Revenue generated")]
        public double RevenueGenerated { get; set; }
        [Name("Customer demographics")]
        public string CustomerDemographics { get; set; } = string.Empty;
        [Name("Stock levels")]
        public int StockLevels { get; set; }
        [Name("Lead times")]
        public int LeadTimes { get; set; }
        [Name("Order quantities")]
        public int OrderQuantities { get; set; }
        [Name("Shipping times")]
        public int ShippingTimes { get; set; }
        [Name("Shipping carriers")]
        public string ShippingCarriers { get; set; } = string.Empty;
        [Name("Shipping costs")]
        public double ShippingCosts { get; set; }
        [Name("Supplier name")]
        public string SupplierName { get; set; } = string.Empty;
        [Name("Location")]
        public string Location { get; set; } = string.Empty;
        [Name("Lead time")]
        public int LeadTime { get; set; }
        [Name("Production volumes")]
        public int ProductionVolumes { get; set; }
        [Name("Manufacturing lead time")]
        public int ManufacturingLeadTime { get; set; }
        [Name("Manufacturing costs")]
        public double ManufacturingCosts { get; set; }
        [Name("Inspection results")]
        public string InspectionResults { get; set; } = string.Empty;
        [Name("Defect rates")]
        public double DefectRates { get; set; }
        [Name("Transportation modes")]
        public string TransportationModes { get; set; } = string.Empty;
        [Name("Routes")]
        public string Routes { get; set; } = string.Empty;
        [Name("Costs")]
        public double Costs { get; set; }
        [Name("is_weekend")]
        public int IsWeekend { get; set; }
        [Name("day_of_week")]
        public int DayOfWeek { get; set; }
        [Name("month")]
        public int Month { get; set; }
        [Name("quarter")]
        public int Quarter { get; set; }
        [Name("year")]
        public int Year { get; set; }
        [Name("is_holiday")]
        public int IsHoliday { get; set; }
        [Name("holiday_name")]
        public string HolidayName { get; set; } = string.Empty;
        [Name("is_event")]
        public int IsEvent { get; set; }
        [Name("event_name")]
        public string EventName { get; set; } = string.Empty;
        [Name("seasonal_multiplier")]
        public float SeasonalMultiplier { get; set; }
    }

    public class DemandData
    {
        public DateTime Date { get; set; }
        public float Demand { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public float Price { get; set; }
        public int StockLevels { get; set; }
        public int LeadTimes { get; set; }
        public int DayOfWeek { get; set; }
        public int Month { get; set; }
        public int Quarter { get; set; }
        public int DayOfYear { get; set; }
        public int WeekOfYear { get; set; }
        public float PriceChange { get; set; }
        public float StockRatio { get; set; }
        public float DemandAcceleration { get; set; }
        public int IsWeekend { get; set; }
        public int Year { get; set; }
        public int IsHoliday { get; set; }
        public string HolidayName { get; set; } = string.Empty;
        public int IsEvent { get; set; }
        public string EventName { get; set; } = string.Empty;
        public float SeasonalMultiplier { get; set; }

      
        public TransformationInfo? TransformationInfo { get; set; }
    }

    public class TransformationInfo
    {
        public string TransformationType { get; set; } = "None"; 
        public float Lambda { get; set; } = 0.5f; 
        public List<float> OriginalSeries { get; set; } = new();
    }
    public class EnhancedXgBoostInput
    {
        public float Lag1 { get; set; }
        public float Lag2 { get; set; }
        public float Lag3 { get; set; }
        public float Lag7 { get; set; }
        public float MovingAvg3 { get; set; }
        public float MovingAvg7 { get; set; }
        public float MovingAvg14 { get; set; }
        public float MovingAvg21 { get; set; }
        public float ExponentialSmoothing { get; set; }
        public float Volatility { get; set; }
        public float Trend { get; set; }
        public float DayOfWeek { get; set; }
        public float Month { get; set; }
        public float Quarter { get; set; }
        public float DayOfYear { get; set; }
        public float WeekOfYear { get; set; }
        public float Price { get; set; }
        public float PriceChange { get; set; }
        public float StockLevels { get; set; }
        public float StockRatio { get; set; }
        public float LeadTimes { get; set; }
        public float ProphetForecast { get; set; }
        public float IsWeekend { get; set; }
        public float IsMonthEnd { get; set; }
        public float IsQuarterEnd { get; set; }
        public float RollingStd7 { get; set; }
        public float MovingAvgDiff { get; set; }
        public float Lag1Diff { get; set; }
        public float Lag7Diff { get; set; }
        public float Demand { get; set; }
    }


    public class OptimizedHyperparameters
    {
        public double LearningRate { get; set; } = 0.1;
        public int MaxDepth { get; set; } = 6;
        public int NumTrees { get; set; } = 100;
        public double Subsample { get; set; } = 0.8;
        public double ColsampleBytree { get; set; } = 0.8;
        public double MinChildWeight { get; set; } = 1.0;
        public double RegAlpha { get; set; } = 0.0;
        public double RegLambda { get; set; } = 1.0;
    }

    public class EvaluationMetrics
    {
        public double MAPE { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public double R2 { get; set; }
        public double SMAPE { get; set; }
        public double MASE { get; set; }
    }

    public class ForecastResult
    {
        public string ProductId { get; set; } = string.Empty;
        public List<float> Predictions { get; set; } = new();
        public List<float> ProphetPredictions { get; set; } = new();
        public List<float> XgBoostPredictions { get; set; } = new();
        public string SelectedModel { get; set; } = string.Empty;
        public EvaluationMetrics Metrics { get; set; } = new();
        public OptimizedHyperparameters? OptimalHyperparameters { get; set; }
        public List<float> ConfidenceIntervals { get; set; } = new();
        public double ProphetSMAPE { get; set; }
        public double XgBoostSMAPE { get; set; }
    }

    public class ImprovedDemandForecaster
    {
        private readonly MLContext mlContext;
        private readonly Random random;

        public ImprovedDemandForecaster(int seed = 42)
        {
            mlContext = new MLContext(seed);
            random = new Random(seed);
        }
        public static List<SupplyChainData> LoadCsvData(Stream stream)
        {
            try
            {
                Console.WriteLine("Starting CSV parsing");
                using var reader = new StreamReader(stream);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                Console.WriteLine("CsvReader initialized");
                var records = csv.GetRecords<SupplyChainData>().ToList();
                Console.WriteLine($"Parsed {records.Count} records");
                return records;
            }
            catch (CsvHelper.HeaderValidationException ex)
            {
                Console.WriteLine($"Header validation error: {ex.Message}");
                throw;
            }
            catch (CsvHelperException ex)
            {
                Console.WriteLine($"CSV parsing error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in LoadCsvData: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }
        public List<ForecastResult> ForecastAllProducts(List<SupplyChainData> supplyChainData, int horizon = 7)
        {
            try
            {
                var groupedData = supplyChainData.GroupBy(d => d.SKU);
                var results = new List<ForecastResult>();

                foreach (var group in groupedData)
                {
                    var productData = group.OrderBy(d => d.Date)
                        .Select(d => new DemandData
                        {
                            Date = d.Date,
                            Demand = d.NumberOfProductsSold,
                            ProductId = d.SKU,
                            ProductType = d.ProductType,
                            Price = (float)d.Price,
                            StockLevels = d.StockLevels,
                            LeadTimes = d.LeadTimes,
                            DayOfWeek = d.DayOfWeek,
                            Month = d.Month,
                            Quarter = d.Quarter,
                            DayOfYear = d.Date.DayOfYear,
                            WeekOfYear = GetWeekOfYear(d.Date),
                            IsWeekend = d.IsWeekend,
                            Year = d.Year,
                            IsHoliday = d.IsHoliday,
                            HolidayName = d.HolidayName,
                            IsEvent = d.IsEvent,
                            EventName = d.EventName,
                            SeasonalMultiplier = d.SeasonalMultiplier
                        }).ToList();

                    if (productData.Count >= 21)
                    {
                        var result = ForecastDemand(productData, group.Key, horizon);
                        results.Add(result);
                    }
                    else
                    {
                        Console.WriteLine($"Insufficient data for {group.Key} ({productData.Count} points). Using fallback forecast.");
                        results.Add(CreateFallbackForecast(group.Key, productData, horizon));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data: {ex.Message}");
                throw;
            }
        }




        /////////////////////For Each Product Processing/////////////////////
        private static int GetWeekOfYear(DateTime date)
        {
            var firstDayOfYear = new DateTime(date.Year, 1, 1);
            var daysOffset = DayOfWeek.Monday - firstDayOfYear.DayOfWeek;
            var firstMonday = firstDayOfYear.AddDays(daysOffset);
            var calendarWeek = ((date - firstMonday).Days / 7) + 1;
            return Math.Max(1, Math.Min(52, calendarWeek));
        }
        public ForecastResult ForecastDemand(IEnumerable<DemandData> data, string productId, int horizon = 7)
        {
            var productData = data.OrderBy(d => d.Date).ToList();
            if (productData.Count < 21)
                throw new ArgumentException($"Insufficient data for product {productId}.");

            var (processedData, transformationInfo) = EnhancedPreprocessDataWithTransformation(productData);
            var trainRatio = Math.Max(0.7, Math.Min(0.9, 1.0 - (14.0 / processedData.Count)));
            var trainSize = (int)(processedData.Count * trainRatio);
            var validationSize = processedData.Count - trainSize;

            Console.WriteLine($"Product {productId}: Training on {trainSize} samples, validating on {validationSize} samples");

            var requiredHorizon = validationSize + horizon;
            var prophetForecasts = RunProphetForecasting(processedData.Take(trainSize).ToList(), processedData.First().ProductType, requiredHorizon);
            var prophetValidationForecasts = prophetForecasts.Take(validationSize).ToList();
            var prophetFutureForecasts = prophetForecasts.Skip(validationSize).Take(horizon).ToList();

            var xgBoostTrainData = PrepareEnhancedXgBoostData(processedData, 21, trainSize - 21);
            var xgBoostValidationData = PrepareEnhancedXgBoostData(processedData, trainSize, validationSize, prophetValidationForecasts);

            if (xgBoostTrainData.Count == 0 || xgBoostValidationData.Count == 0)
            {
                Console.WriteLine($"Warning: Insufficient data for XGBoost training for product {productId}");
                return CreateFallbackForecast(productId, processedData, horizon);
            }

            var optimalParams = RunEnhancedBayesianOptimization(xgBoostTrainData, xgBoostValidationData, 150);
            var xgBoostModel = TrainXgBoost(xgBoostTrainData, optimalParams);

            var xgBoostValidationPredictions = PredictWithXgBoost(xgBoostModel, xgBoostValidationData);
            var xgBoostFutureData = CreateEnhancedFutureFeatures(processedData, prophetFutureForecasts, horizon);
            var xgBoostFuturePredictions = PredictWithXgBoost(xgBoostModel, xgBoostFutureData);

            var actualValidation = processedData.Skip(trainSize).Select(d => d.Demand).ToList();


            var prophetSMAPE = CalculateSMAPE(prophetValidationForecasts, actualValidation);
            var xgBoostSMAPE = CalculateSMAPE(xgBoostValidationPredictions, actualValidation);


            Console.WriteLine($"Model Performance Comparison for {productId}:");
            Console.WriteLine($"  Prophet SMAPE: {prophetSMAPE:F4}%");
            Console.WriteLine($"  XGBoost SMAPE: {xgBoostSMAPE:F4}%");



            var minSMAPE = Math.Min(prophetSMAPE, xgBoostSMAPE);
            string selectedModel;
            List<float> selectedPredictions;
            List<float> selectedValidationPredictions;

            if (minSMAPE == prophetSMAPE)
            {
                selectedModel = "Prophet";
                selectedPredictions = prophetFutureForecasts;
                selectedValidationPredictions = prophetValidationForecasts;
                Console.WriteLine($"  Selected Model: Prophet (SMAPE: {prophetSMAPE:F4}%)");
            }
            else
            {
                selectedModel = "XGBoost";
                selectedPredictions = xgBoostFuturePredictions;
                selectedValidationPredictions = xgBoostValidationPredictions;
                Console.WriteLine($"  Selected Model: XGBoost (SMAPE: {xgBoostSMAPE:F4}%)");
            }


            var metrics = CalculateComprehensiveMetrics(
                actualValidation,
                selectedValidationPredictions,
                processedData.Take(trainSize).Select(d => d.Demand).ToList()
            );

            if (!ValidateDataQuality(productData))
            {
                Console.WriteLine($"Unstable data pattern for {productId}, using fallback");
                return CreateFallbackForecast(productId, productData, horizon);
            }

            var revertedPredictions = InverseTransformation(selectedPredictions, transformationInfo);
            var revertedProphetPredictions = InverseTransformation(prophetFutureForecasts, transformationInfo);
            var revertedXgBoostPredictions = InverseTransformation(xgBoostFuturePredictions, transformationInfo);
            var revertedConfidenceIntervals = CalculateConfidenceIntervals(revertedPredictions, metrics.RMSE);

            return new ForecastResult
            {
                ProductId = productId,
                Predictions = revertedPredictions,
                ProphetPredictions = revertedProphetPredictions,
                XgBoostPredictions = revertedXgBoostPredictions,
                Metrics = metrics,
                OptimalHyperparameters = optimalParams,
                ConfidenceIntervals = revertedConfidenceIntervals,
                SelectedModel = selectedModel,
                ProphetSMAPE = prophetSMAPE,
                XgBoostSMAPE = xgBoostSMAPE
            };
        }
        private static (List<DemandData> processedData, TransformationInfo transformationInfo) EnhancedPreprocessDataWithTransformation(List<DemandData> data)
        {
            var processedData = new List<DemandData>();
            var demands = data.Select(d => d.Demand).ToList();


            var (transformedDemands, transformationInfo) = ApplyOptimalTransformationWithInfo(demands);

            for (int i = 0; i < data.Count; i++)
            {
                var item = data[i];
                var cleanedDemand = transformedDemands[i];

                var priceChange = i > 0 ? item.Price - data[i - 1].Price : 0;
                var stockRatio = item.StockLevels > 0 ? cleanedDemand / item.StockLevels : 0;
                var demandAcceleration = CalculateDemandAcceleration(transformedDemands, i);

                processedData.Add(new DemandData
                {
                    Date = item.Date,
                    Demand = Math.Max(0.1f, cleanedDemand),
                    ProductId = item.ProductId,
                    ProductType = item.ProductType,
                    Price = Math.Max(0.01f, item.Price),
                    StockLevels = Math.Max(1, item.StockLevels),
                    LeadTimes = Math.Max(1, item.LeadTimes),
                    DayOfWeek = item.DayOfWeek,
                    Month = item.Month,
                    Quarter = item.Quarter,
                    DayOfYear = item.DayOfYear,
                    WeekOfYear = item.WeekOfYear,
                    PriceChange = priceChange,
                    StockRatio = Math.Min(10f, stockRatio),
                    DemandAcceleration = demandAcceleration,
                    IsWeekend = item.IsWeekend,
                    Year = item.Year,
                    IsHoliday = item.IsHoliday,
                    HolidayName = item.HolidayName,
                    IsEvent = item.IsEvent,
                    EventName = item.EventName,
                    SeasonalMultiplier = item.SeasonalMultiplier,
                    TransformationInfo = transformationInfo
                });
            }

            return (processedData, transformationInfo);
        }
        private static (List<float> transformedSeries, TransformationInfo transformationInfo) ApplyOptimalTransformationWithInfo(List<float> series)
        {
            var original = series.ToList();
            var logTransformed = series.Select(x => (float)Math.Log(x + 1)).ToList();
            var sqrtTransformed = series.Select(x => (float)Math.Sqrt(x)).ToList();
            var lambda = 0.5f;
            var boxCoxTransformed = ApplyBoxCoxTransformation(series, lambda);

            var originalSkewness = CalculateSkewness(original);
            var logSkewness = CalculateSkewness(logTransformed);
            var sqrtSkewness = CalculateSkewness(sqrtTransformed);
            var boxCoxSkewness = CalculateSkewness(boxCoxTransformed);

            var transformations = new[]
            {
        (Math.Abs(originalSkewness), original, "None", 0f),
        (Math.Abs(logSkewness), logTransformed, "Log", 0f),
        (Math.Abs(sqrtSkewness), sqrtTransformed, "Sqrt", 0f),
        (Math.Abs(boxCoxSkewness), boxCoxTransformed, "BoxCox", lambda)
    };

            var bestTransformation = transformations.OrderBy(t => t.Item1).First();

            var transformationInfo = new TransformationInfo
            {
                TransformationType = bestTransformation.Item3,
                Lambda = bestTransformation.Item4,
                OriginalSeries = original
            };

            return (bestTransformation.Item2, transformationInfo);
        }
        private static float CalculateSkewness(List<float> series)
        {
            var mean = series.Average();
            var std = (float)Math.Sqrt(series.Select(x => Math.Pow(x - mean, 2)).Average());

            if (std < 1e-10) return 0;

            var skewness = series.Select(x => Math.Pow((x - mean) / std, 3)).Average();
            return (float)skewness;
        }
        private static List<float> ApplyBoxCoxTransformation(List<float> series, float lambda)
        {
            if (Math.Abs(lambda) < 1e-10)
            {
                return series.Select(x => (float)Math.Log(x + 1)).ToList();
            }

            return series.Select(x => (float)((Math.Pow(x + 1, lambda) - 1) / lambda)).ToList();
        }
        private static float CalculateDemandAcceleration(List<float> series, int index)
        {
            if (index < 2) return 0;

            var current = series[index];
            var previous = series[index - 1];
            var beforePrevious = series[index - 2];

            var velocity1 = current - previous;
            var velocity2 = previous - beforePrevious;

            return velocity1 - velocity2;
        }



        /////////////////////Model Training and Prediction/////////////////////
        private List<float> RunProphetForecasting(List<DemandData> trainData, string productType, int requiredHorizon)
        {
            try
            {
                var historicalData = trainData.Select(d => new
                {
                    ds = d.Date.ToString("yyyy-MM-dd"),
                    y = d.Demand,
                    is_weekend = d.IsWeekend,
                    day_of_week = d.DayOfWeek,
                    month = d.Month,
                    quarter = d.Quarter,
                    year = d.Year,
                    seasonal_multiplier = d.SeasonalMultiplier
                }).ToList();

                var inputData = new
                {
                    historical_data = historicalData,
                    product_type = productType,
                    required_horizon = requiredHorizon
                };
                var jsonInput = JsonConvert.SerializeObject(inputData);

                var tempFilePath = Path.GetTempFileName();
                File.WriteAllText(tempFilePath, jsonInput);
                Console.WriteLine($"JSON input size: {jsonInput.Length} characters");
                Console.WriteLine($"Temporary file created at: {tempFilePath}");

                var pythonPath = @"C:\Users\yosri\AppData\Local\Microsoft\WindowsApps\python.exe";
                var scriptPath = @"C:\Users\yosri\Desktop\projects for me\intership 4éme\MLINTERNSHIP\prophet_forecast.py";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\" \"{tempFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    var timeout = TimeSpan.FromMinutes(5);
                    if (!process.WaitForExit((int)timeout.TotalMilliseconds))
                    {
                        Console.WriteLine("Python process timed out, killing process");
                        process.Kill();
                        File.Delete(tempFilePath);
                        return GenerateExponentialSmoothingForecast(trainData, requiredHorizon);
                    }

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    Console.WriteLine($"Python process exit code: {process.ExitCode}");
                    Console.WriteLine($"Python stdout: {output}");
                    Console.WriteLine($"Python stderr: {error}");
                    File.Delete(tempFilePath);

                    if (process.ExitCode != 0)
                    {
                        Console.WriteLine($"Prophet forecasting failed with exit code {process.ExitCode}: {error}");
                        return GenerateExponentialSmoothingForecast(trainData, requiredHorizon);
                    }

                    if (string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine("Prophet returned empty output");
                        return GenerateExponentialSmoothingForecast(trainData, requiredHorizon);
                    }

                    var outputData = JsonConvert.DeserializeObject<Dictionary<string, List<float>>>(output);
                    if (outputData == null || !outputData.ContainsKey("forecasts"))
                    {
                        Console.WriteLine("Invalid output format from Python script");
                        return GenerateExponentialSmoothingForecast(trainData, requiredHorizon);
                    }

                    return outputData["forecasts"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RunProphetForecasting: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return GenerateExponentialSmoothingForecast(trainData, requiredHorizon);
            }
        }
        private static List<float> GenerateExponentialSmoothingForecast(List<DemandData> data, int horizon)
        {
            var alpha = 0.3f;
            var beta = 0.1f;
            var demands = data.Select(d => d.Demand).ToList();
            var level = demands.First();
            var trend = demands.Count > 1 ? demands[1] - demands[0] : 0;

            for (int i = 1; i < demands.Count; i++)
            {
                var prevLevel = level;
                level = alpha * demands[i] + (1 - alpha) * (level + trend);
                trend = beta * (level - prevLevel) + (1 - beta) * trend;
            }

            var forecasts = new List<float>();
            for (int i = 0; i < horizon; i++)
            {
                forecasts.Add(Math.Max(0, level + trend * (i + 1)));
            }
            return forecasts;
        }
        private static List<EnhancedXgBoostInput> PrepareEnhancedXgBoostData(List<DemandData> fullData, int startIndex, int count, List<float>? prophetForecasts = null)
        {
            var xgBoostData = new List<EnhancedXgBoostInput>();
            for (int i = startIndex; i < startIndex + count && i < fullData.Count; i++)
            {
                if (i < 21) continue;

                var demand = fullData[i].Demand;
                var recentDemands = fullData.Skip(i - 21).Take(21).Select(d => d.Demand).ToList();
                var movingAvg3 = recentDemands.TakeLast(3).Average();
                var movingAvg7 = recentDemands.TakeLast(7).Average();
                var movingAvg14 = recentDemands.TakeLast(14).Average();
                var movingAvg21 = recentDemands.Average();
                var exponentialSmoothing = CalculateExponentialSmoothing(recentDemands, 0.3f);
                var volatility = (float)Math.Sqrt(recentDemands.TakeLast(7).Select(d => Math.Pow(d - movingAvg7, 2)).Average());
                var trend = recentDemands.Count >= 7 ? (recentDemands.TakeLast(3).Average() - recentDemands.Take(3).Average()) / 18f : 0;
                var prophetForecast = (prophetForecasts != null && i - startIndex < prophetForecasts.Count) ? prophetForecasts[i - startIndex] : 0;
                var rollingStd7 = (float)StandardDeviation(recentDemands.TakeLast(7));
                var movingAvgDiff = movingAvg7 - movingAvg21;
                var lag1Diff = demand - fullData[i - 1].Demand;
                var lag7Diff = i >= 7 ? demand - fullData[i - 7].Demand : 0;

                xgBoostData.Add(new EnhancedXgBoostInput
                {
                    Lag1 = fullData[i - 1].Demand,
                    Lag2 = fullData[i - 2].Demand,
                    Lag3 = fullData[i - 3].Demand,
                    Lag7 = i >= 7 ? fullData[i - 7].Demand : fullData[i - 1].Demand,
                    MovingAvg3 = movingAvg3,
                    MovingAvg7 = movingAvg7,
                    MovingAvg14 = movingAvg14,
                    MovingAvg21 = movingAvg21,
                    ExponentialSmoothing = exponentialSmoothing,
                    Volatility = volatility,
                    Trend = trend,
                    DayOfWeek = fullData[i].DayOfWeek,
                    Month = fullData[i].Month,
                    Quarter = fullData[i].Quarter,
                    DayOfYear = fullData[i].DayOfYear,
                    WeekOfYear = fullData[i].WeekOfYear,
                    Price = fullData[i].Price,
                    PriceChange = fullData[i].PriceChange,
                    StockLevels = fullData[i].StockLevels,
                    StockRatio = fullData[i].StockRatio,
                    LeadTimes = fullData[i].LeadTimes,
                    ProphetForecast = prophetForecast,
                    IsWeekend = fullData[i].DayOfWeek == 0 || fullData[i].DayOfWeek == 6 ? 1 : 0,
                    IsMonthEnd = fullData[i].Date.Day >= 28 ? 1 : 0,
                    IsQuarterEnd = fullData[i].Month % 3 == 0 && fullData[i].Date.Day >= 28 ? 1 : 0,
                    RollingStd7 = rollingStd7,
                    MovingAvgDiff = movingAvgDiff,
                    Lag1Diff = lag1Diff,
                    Lag7Diff = lag7Diff,
                    Demand = demand
                });
            }
            return xgBoostData;
        }
        private static float CalculateExponentialSmoothing(List<float> data, float alpha)
        {
            if (data.Count == 0) return 0;
            var smoothed = data[0];
            for (int i = 1; i < data.Count; i++)
            {
                smoothed = alpha * data[i] + (1 - alpha) * smoothed;
            }
            return smoothed;
        }
        private static double StandardDeviation(IEnumerable<float> values)
        {
            var avg = values.Average();
            var sum = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sum / values.Count());
        }
        private OptimizedHyperparameters RunEnhancedBayesianOptimization(List<EnhancedXgBoostInput> trainData, List<EnhancedXgBoostInput> validationData, int iterations)
        {
            var bestParams = new OptimizedHyperparameters();
            var bestError = double.MaxValue;

            for (int i = 0; i < iterations; i++)
            {
                var candidateParams = new OptimizedHyperparameters
                {
                    LearningRate = 0.01 + random.NextDouble() * 0.29,
                    MaxDepth = random.Next(3, 11),
                    NumTrees = random.Next(50, 501),
                    Subsample = 0.6 + random.NextDouble() * 0.4,
                    ColsampleBytree = 0.6 + random.NextDouble() * 0.4,
                    MinChildWeight = random.Next(1, 6),
                    RegAlpha = random.NextDouble() * 0.1,
                    RegLambda = random.NextDouble() * 2
                };

                try
                {
                    var model = TrainXgBoost(trainData, candidateParams);
                    var predictions = PredictWithXgBoost(model, validationData);
                    var actual = validationData.Select(d => d.Demand).ToList();
                    var error = CalculateSMAPE(predictions, actual);

                    if (error < bestError)
                    {
                        bestError = error;
                        bestParams = candidateParams;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Optimization iteration {i} failed: {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"Best hyperparameters found with SMAPE: {bestError:F4}");
            return bestParams;
        }
        private XGBRegressor TrainXgBoost(List<EnhancedXgBoostInput> trainData, OptimizedHyperparameters hyperparams)
        {
            float[][] dataTrain = trainData.Select(d => new float[]
            {
        d.Lag1, d.Lag2, d.Lag3, d.Lag7, d.MovingAvg3, d.MovingAvg7, d.MovingAvg14, d.MovingAvg21,
        d.ExponentialSmoothing, d.Volatility, d.Trend, d.DayOfWeek, d.Month, d.Quarter, d.DayOfYear,
        d.WeekOfYear, d.Price, d.PriceChange, d.StockLevels, d.StockRatio, d.LeadTimes, d.ProphetForecast,
        d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd, d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
            }).ToArray();
            float[] labelsTrain = trainData.Select(d => d.Demand).ToArray();

            var regressor = new XGBRegressor(
                learningRate: (float)hyperparams.LearningRate,
                maxDepth: hyperparams.MaxDepth,
                nEstimators: hyperparams.NumTrees,
                subsample: (float)hyperparams.Subsample,
                colSampleByTree: (float)hyperparams.ColsampleBytree,
                minChildWeight: (int)hyperparams.MinChildWeight,
                regAlpha: (float)hyperparams.RegAlpha,
                regLambda: (float)hyperparams.RegLambda
            );

            regressor.Fit(dataTrain, labelsTrain);
            return regressor;
        }
        private List<float> PredictWithXgBoost(XGBRegressor regressor, List<EnhancedXgBoostInput> inputs)
        {
            float[][] data = inputs.Select(d => new float[]
            {
        d.Lag1, d.Lag2, d.Lag3, d.Lag7, d.MovingAvg3, d.MovingAvg7, d.MovingAvg14, d.MovingAvg21,
        d.ExponentialSmoothing, d.Volatility, d.Trend, d.DayOfWeek, d.Month, d.Quarter, d.DayOfYear,
        d.WeekOfYear, d.Price, d.PriceChange, d.StockLevels, d.StockRatio, d.LeadTimes, d.ProphetForecast,
        d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd, d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
            }).ToArray();
            float[] predictions = regressor.Predict(data);
            return predictions.Select(p => Math.Max(0, p)).ToList();
        }
        private static List<EnhancedXgBoostInput> CreateEnhancedFutureFeatures(List<DemandData> historicalData, List<float> prophetForecasts, int horizon)
        {
            var futureFeatures = new List<EnhancedXgBoostInput>();
            var lastDate = historicalData.Last().Date;
            var recentDemands = historicalData.TakeLast(21).Select(d => d.Demand).ToList();

            for (int i = 0; i < horizon; i++)
            {
                var futureDate = lastDate.AddDays(i + 1);
                var currentDemands = new List<float>(recentDemands);
                if (i > 0) currentDemands.AddRange(prophetForecasts.Take(i));

                var movingAvg3 = currentDemands.TakeLast(3).Average();
                var movingAvg7 = currentDemands.TakeLast(7).Average();
                var movingAvg14 = currentDemands.TakeLast(14).Average();
                var movingAvg21 = currentDemands.TakeLast(21).Average();
                var exponentialSmoothing = CalculateExponentialSmoothing(currentDemands.TakeLast(21).ToList(), 0.3f);
                var volatility = (float)Math.Sqrt(currentDemands.TakeLast(7).Select(d => Math.Pow(d - movingAvg7, 2)).Average());
                var trend = currentDemands.Count >= 7 ? (currentDemands.TakeLast(3).Average() - currentDemands.Take(3).Average()) / 18f : 0;
                var rollingStd7 = (float)StandardDeviation(currentDemands.TakeLast(7));
                var movingAvgDiff = movingAvg7 - movingAvg21;
                var lag1Diff = i > 0 ? prophetForecasts[i - 1] - currentDemands.Last() : currentDemands.Last() - currentDemands[^2];
                var lag7Diff = i >= 6 ? prophetForecasts[i - 1] - currentDemands[^7] : 0;

                futureFeatures.Add(new EnhancedXgBoostInput
                {
                    Lag1 = currentDemands.Last(),
                    Lag2 = currentDemands.Count > 1 ? currentDemands[^2] : 0,
                    Lag3 = currentDemands.Count > 2 ? currentDemands[^3] : 0,
                    Lag7 = currentDemands.Count > 6 ? currentDemands[^7] : currentDemands.Last(),
                    MovingAvg3 = movingAvg3,
                    MovingAvg7 = movingAvg7,
                    MovingAvg14 = movingAvg14,
                    MovingAvg21 = movingAvg21,
                    ExponentialSmoothing = exponentialSmoothing,
                    Volatility = volatility,
                    Trend = trend,
                    DayOfWeek = (float)futureDate.DayOfWeek,
                    Month = futureDate.Month,
                    Quarter = (futureDate.Month - 1) / 3 + 1,
                    DayOfYear = futureDate.DayOfYear,
                    WeekOfYear = GetWeekOfYear(futureDate),
                    Price = historicalData.Last().Price,
                    PriceChange = 0,
                    StockLevels = historicalData.Last().StockLevels,
                    StockRatio = historicalData.Last().StockRatio,
                    LeadTimes = historicalData.Last().LeadTimes,
                    ProphetForecast = prophetForecasts.ElementAtOrDefault(i),
                    IsWeekend = futureDate.DayOfWeek == DayOfWeek.Saturday || futureDate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0,
                    IsMonthEnd = futureDate.Day >= 28 ? 1 : 0,
                    IsQuarterEnd = futureDate.Month % 3 == 0 && futureDate.Day >= 28 ? 1 : 0,
                    RollingStd7 = rollingStd7,
                    MovingAvgDiff = movingAvgDiff,
                    Lag1Diff = lag1Diff,
                    Lag7Diff = lag7Diff,
                    Demand = 0
                });
            }
            return futureFeatures;
        }



        /////////////////////Model Evaluation and Selection/////////////////////
        private static double CalculateSMAPE(List<float> predictions, List<float> actual)
        {
            if (predictions.Count != actual.Count)
                throw new ArgumentException("Predictions and actual values must have the same length");

            double sum = 0;
            for (int i = 0; i < predictions.Count; i++)
            {
                var a = actual[i];
                var p = predictions[i];
                sum += 2 * Math.Abs(a - p) / (Math.Abs(a) + Math.Abs(p) + 1e-10);
            }
            return (sum / predictions.Count) * 100;
        }
        private static EvaluationMetrics CalculateComprehensiveMetrics(List<float> actual, List<float> predicted, List<float> trainData)
        {
            if (actual.Count != predicted.Count)
                throw new ArgumentException("Actual and predicted values must have the same length");

            var n = actual.Count;
            var actualMean = actual.Average();

            var mape = actual.Zip(predicted, (a, p) => a == 0 ? 0 : Math.Abs((a - p) / a))
                            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
                            .Average() * 100;
            var rmse = Math.Sqrt(actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Average());
            var mae = actual.Zip(predicted, (a, p) => Math.Abs(a - p)).Average();
            var ss_res = actual.Zip(predicted, (a, p) => Math.Pow(a - p, 2)).Sum();
            var ss_tot = actual.Select(a => Math.Pow(a - actualMean, 2)).Sum();
            var r2 = ss_tot == 0 ? 1.0 : 1 - (ss_res / ss_tot);
            var smape = actual.Zip(predicted, (a, p) => 2 * Math.Abs(a - p) / (Math.Abs(a) + Math.Abs(p) + 1e-10)).Average() * 100;
            var naiveForecastError = trainData.Skip(1).Zip(trainData, (curr, prev) => Math.Abs(curr - prev)).Average();
            var mase = mae / (naiveForecastError + 1e-10);

            return new EvaluationMetrics
            {
                MAPE = mape,
                RMSE = rmse,
                MAE = mae,
                R2 = r2,
                SMAPE = smape,
                MASE = mase
            };
        }
        private static bool ValidateDataQuality(List<DemandData> data)
        {
            var mean = data.Average(d => d.Demand);
            var stdDev = Math.Sqrt(data.Select(d => Math.Pow(d.Demand - mean, 2)).Average());
            return stdDev / mean < 2.0;
        }





        /////////////////////Post-Processing/////////////////////
        private static List<float> InverseTransformation(List<float> transformedValues, TransformationInfo transformationInfo)
        {
            return transformationInfo.TransformationType switch
            {
                "Log" => transformedValues.Select(x => (float)(Math.Exp(x) - 1)).ToList(),
                "Sqrt" => transformedValues.Select(x => x * x).ToList(),
                "BoxCox" => InverseBoxCoxTransformation(transformedValues, transformationInfo.Lambda),
                "None" => transformedValues.ToList(),
                _ => transformedValues.ToList()
            };
        }
        private static List<float> InverseBoxCoxTransformation(List<float> transformedSeries, float lambda)
        {
            if (Math.Abs(lambda) < 1e-10)
            {
                return transformedSeries.Select(x => (float)(Math.Exp(x) - 1)).ToList();
            }

            return transformedSeries.Select(x => (float)(Math.Pow(lambda * x + 1, 1.0 / lambda) - 1)).ToList();
        }
        private static List<float> CalculateConfidenceIntervals(List<float> predictions, double rmse)
        {
            const float zScore = 1.96f;
            return predictions.Select(p => (float)rmse * zScore).ToList();
        }
        private ForecastResult CreateFallbackForecast(string productId, List<DemandData> data, int horizon)
        {
          
            var transformationInfo = data.FirstOrDefault()?.TransformationInfo ?? new TransformationInfo();

            var forecasts = GenerateExponentialSmoothingForecast(data, horizon);
            var revertedForecasts = InverseTransformation(forecasts, transformationInfo);

            return new ForecastResult
            {
                ProductId = productId,
                Predictions = revertedForecasts,
                ProphetPredictions = revertedForecasts,
                XgBoostPredictions = revertedForecasts,
                Metrics = new EvaluationMetrics { MAPE = double.NaN, RMSE = double.NaN, MAE = double.NaN, R2 = double.NaN, SMAPE = double.NaN, MASE = double.NaN },
                OptimalHyperparameters = null,
                ConfidenceIntervals = Enumerable.Repeat(0f, horizon).ToList(),
                SelectedModel = "ExponentialSmoothing"
            };
        }

    }

    public static class Extensions
    {
        public static double Variance(this IEnumerable<double> source)
        {
            var n = 0;
            double mean = 0;
            double m2 = 0;

            foreach (var x in source)
            {
                n++;
                var delta = x - mean;
                mean += delta / n;
                var delta2 = x - mean;
                m2 += delta * delta2;
            }

            return n > 1 ? m2 / (n - 1) : 0;
        }
    }

    public class Program
    {
        public static void Main()
        {
            var forecaster = new ImprovedDemandForecaster();
            string csvPath = @"C:\Users\yosri\Desktop\projects for me\intership 4éme\MLINTERNSHIP\supply_chain_data.csv";

            try
            {
                
                List<SupplyChainData> supplyChainData;
                using (var stream = new FileStream(csvPath, FileMode.Open, FileAccess.Read))
                {
                    supplyChainData = ImprovedDemandForecaster.LoadCsvData(stream);
                }


                var results = forecaster.ForecastAllProducts(supplyChainData, horizon: 7);

                foreach (var result in results)
                {
                    Console.WriteLine($"\n{'=' * 60}");
                    Console.WriteLine($"FORECAST RESULTS FOR {result.ProductId}");
                    Console.WriteLine($"{'=' * 60}");

                    Console.WriteLine($"\nModel Performance Comparison:");
                    Console.WriteLine($"  Prophet SMAPE:    {result.ProphetSMAPE:F4}%");
                    Console.WriteLine($"  XGBoost SMAPE:    {result.XgBoostSMAPE:F4}%");
                    Console.WriteLine($"\n>>> SELECTED MODEL: {result.SelectedModel} <<<");

                    Console.WriteLine($"\nOverall Metrics (Selected Model):");
                    Console.WriteLine($"  MAPE:  {result.Metrics.MAPE:F2}%");
                    Console.WriteLine($"  RMSE:  {result.Metrics.RMSE:F2}");
                    Console.WriteLine($"  MAE:   {result.Metrics.MAE:F2}");
                    Console.WriteLine($"  R²:    {result.Metrics.R2:F4}");
                    Console.WriteLine($"  SMAPE: {result.Metrics.SMAPE:F2}%");
                    Console.WriteLine($"  MASE:  {result.Metrics.MASE:F4}");

                    Console.WriteLine($"\nForecast for next 7 days:");
                    for (int i = 0; i < result.Predictions.Count; i++)
                    {
                        Console.WriteLine($"  Day {i + 1}: {result.Predictions[i]:F2} (±{result.ConfidenceIntervals[i]:F2})");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
