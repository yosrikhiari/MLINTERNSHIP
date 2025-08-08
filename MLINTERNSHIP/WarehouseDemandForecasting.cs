using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;
using XGBoostSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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

        [Name("Procurement lead time")]
        public int ProcurementLeadTime { get; set; }

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

        [Name("Manufacturing lead time")]
        public int ManufacturingLeadTime { get; set; }

        [Name("Manufacturing costs")]
        public double ManufacturingCosts { get; set; }

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
    }

    public class DemandData
    {
        public DateTime Date { get; set; }
        public float Demand { get; set; }
        public string ProductId { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public float Price { get; set; }
        public int StockLevels { get; set; }
        public int ManufacturingLeadTime { get; set; }
        public int ProcurementLeadTime { get; set; }
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
        public float ProphetSeasonalSpike { get; set; }


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
        public float ProcurementLeadTime { get; set; }
        public float ManufacturingLeadTime { get; set; }
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
        public List<float> ProphetPredictions { get; set; } = new(); // ADD THIS LINE
        public List<float> XgBoostPredictions { get; set; } = new();
        public string SelectedModel { get; set; } = string.Empty;
        public EvaluationMetrics Metrics { get; set; } = new();
        public OptimizedHyperparameters? OptimalHyperparameters { get; set; }
        public List<float> ConfidenceIntervals { get; set; } = new();
        public double XgBoostSMAPE { get; set; }
        public double ProphetSMAPE { get; set; } // ADD THIS LINE TOO
    }
    public class EnhancedForecastResult : ForecastResult
    {
        public ForecastRequest Request { get; set; } = new();
        public List<DateTime> ForecastDates { get; set; } = new();
        public List<(DateTime Date, float Value, float ConfidenceInterval)> DetailedForecasts { get; set; } = new();
        public List<float> AggregatedPredictions { get; set; } = new();
        public List<float> AggregatedConfidenceIntervals { get; set; } = new();
        public List<string> AggregatedLabels { get; set; } = new();
    }
    public enum ForecastUnit
    {
        Days = 1,
        Weeks = 7,
        Months = 30 // Approximate, will be calculated more precisely
    }

    public class ForecastRequest
    {
        public ForecastUnit Unit { get; set; } = ForecastUnit.Days;
        public int Quantity { get; set; } = 7;
        public int HorizonInDays => CalculateHorizonInDays();

        private int CalculateHorizonInDays()
        {
            return Unit switch
            {
                ForecastUnit.Days => Quantity,
                ForecastUnit.Weeks => Quantity * 7,
                ForecastUnit.Months => CalculateMonthsInDays(),
                _ => Quantity
            };
        }

        private int CalculateMonthsInDays()
        {
            // More precise month calculation
            var startDate = DateTime.Now;
            var endDate = startDate.AddMonths(Quantity);
            return (int)(endDate - startDate).TotalDays;
        }

        public string GetDisplayText()
        {
            return Unit switch
            {
                ForecastUnit.Days => $"{Quantity} day{(Quantity > 1 ? "s" : "")}",
                ForecastUnit.Weeks => $"{Quantity} week{(Quantity > 1 ? "s" : "")} ({HorizonInDays} days)",
                ForecastUnit.Months => $"{Quantity} month{(Quantity > 1 ? "s" : "")} ({HorizonInDays} days)",
                _ => $"{Quantity} periods"
            };
        }
    }

    // Update ImprovedDemandForecaster class with new methods
    public partial class ImprovedDemandForecaster
    {
        public List<EnhancedForecastResult> ForecastAllProductsWithTimeUnits(
            List<SupplyChainData> supplyChainData,
            ForecastRequest request)
        {
            try
            {
                var groupedData = supplyChainData.GroupBy(d => d.SKU);
                var results = new ConcurrentBag<EnhancedForecastResult>();

                // Use the existing ForecastAllProducts method but with enhanced results
                var baseResults = ForecastAllProducts(supplyChainData, request.HorizonInDays);

                Parallel.ForEach(baseResults, (baseResult) =>
                {
                    var enhancedResult = ConvertToEnhancedResult(baseResult, request, supplyChainData);
                    results.Add(enhancedResult);
                });

                return results.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data with time units: {ex.Message}");
                throw;
            }
        }

        private EnhancedForecastResult ConvertToEnhancedResult(
            ForecastResult baseResult,
            ForecastRequest request,
            List<SupplyChainData> supplyChainData)
        {
            var productData = supplyChainData
                .Where(d => d.SKU == baseResult.ProductId)
                .OrderBy(d => d.Date)
                .ToList();

            var lastDate = productData.Last().Date;

            var enhancedResult = new EnhancedForecastResult
            {
                ProductId = baseResult.ProductId,
                Predictions = baseResult.Predictions,
                ProphetPredictions = baseResult.ProphetPredictions,
                XgBoostPredictions = baseResult.XgBoostPredictions,
                SelectedModel = baseResult.SelectedModel,
                Metrics = baseResult.Metrics,
                OptimalHyperparameters = baseResult.OptimalHyperparameters,
                ConfidenceIntervals = baseResult.ConfidenceIntervals,
                XgBoostSMAPE = baseResult.XgBoostSMAPE,
                ProphetSMAPE = baseResult.ProphetSMAPE,
                Request = request
            };

            // Generate forecast dates
            for (int i = 1; i <= request.HorizonInDays; i++)
            {
                enhancedResult.ForecastDates.Add(lastDate.AddDays(i));
            }

            // Create detailed forecasts
            for (int i = 0; i < baseResult.Predictions.Count; i++)
            {
                enhancedResult.DetailedForecasts.Add((
                    enhancedResult.ForecastDates[i],
                    baseResult.Predictions[i],
                    baseResult.ConfidenceIntervals[i]
                ));
            }

            // Aggregate predictions based on time unit
            AggregateForecasts(enhancedResult, request);

            return enhancedResult;
        }

        private void AggregateForecasts(EnhancedForecastResult result, ForecastRequest request)
        {
            result.AggregatedPredictions.Clear();
            result.AggregatedConfidenceIntervals.Clear();
            result.AggregatedLabels.Clear();

            switch (request.Unit)
            {
                case ForecastUnit.Days:
                    // For days, use daily predictions as-is
                    result.AggregatedPredictions = result.Predictions.ToList();
                    result.AggregatedConfidenceIntervals = result.ConfidenceIntervals.ToList();
                    for (int i = 0; i < request.Quantity; i++)
                    {
                        result.AggregatedLabels.Add($"Day {i + 1}");
                    }
                    break;

                case ForecastUnit.Weeks:
                    AggregateByWeeks(result, request);
                    break;

                case ForecastUnit.Months:
                    AggregateByMonths(result, request);
                    break;
            }
        }

        private void AggregateByWeeks(EnhancedForecastResult result, ForecastRequest request)
        {
            for (int week = 0; week < request.Quantity; week++)
            {
                int startIndex = week * 7;
                int endIndex = Math.Min(startIndex + 7, result.Predictions.Count);

                if (startIndex < result.Predictions.Count)
                {
                    var weekPredictions = result.Predictions.Skip(startIndex).Take(endIndex - startIndex);
                    var weekConfidence = result.ConfidenceIntervals.Skip(startIndex).Take(endIndex - startIndex);

                    // Sum for total demand per week
                    result.AggregatedPredictions.Add(weekPredictions.Sum());
                    // Average confidence interval
                    result.AggregatedConfidenceIntervals.Add(weekConfidence.Average());

                    var startDate = result.ForecastDates[startIndex];
                    result.AggregatedLabels.Add($"Week {week + 1} ({startDate:MMM dd})");
                }
            }
        }

        private void AggregateByMonths(EnhancedForecastResult result, ForecastRequest request)
        {
            var currentDate = result.ForecastDates.First();
            var currentMonth = currentDate.Month;
            var currentYear = currentDate.Year;

            var monthlyPredictions = new List<float>();
            var monthlyConfidence = new List<float>();
            int monthIndex = 0;

            for (int i = 0; i < result.Predictions.Count; i++)
            {
                var forecastDate = result.ForecastDates[i];

                // Check if we've moved to a new month
                if (forecastDate.Month != currentMonth || forecastDate.Year != currentYear)
                {
                    // Aggregate current month
                    if (monthlyPredictions.Any())
                    {
                        result.AggregatedPredictions.Add(monthlyPredictions.Sum());
                        result.AggregatedConfidenceIntervals.Add(monthlyConfidence.Average());

                        var monthName = new DateTime(currentYear, currentMonth, 1).ToString("MMM yyyy");
                        result.AggregatedLabels.Add($"Month {monthIndex + 1} ({monthName})");

                        monthlyPredictions.Clear();
                        monthlyConfidence.Clear();
                        monthIndex++;
                    }

                    currentMonth = forecastDate.Month;
                    currentYear = forecastDate.Year;
                }

                monthlyPredictions.Add(result.Predictions[i]);
                monthlyConfidence.Add(result.ConfidenceIntervals[i]);
            }

            // Don't forget the last month
            if (monthlyPredictions.Any() && monthIndex < request.Quantity)
            {
                result.AggregatedPredictions.Add(monthlyPredictions.Sum());
                result.AggregatedConfidenceIntervals.Add(monthlyConfidence.Average());

                var monthName = new DateTime(currentYear, currentMonth, 1).ToString("MMM yyyy");
                result.AggregatedLabels.Add($"Month {monthIndex + 1} ({monthName})");
            }
        }

        // Validation method for forecast requests
        public static (bool IsValid, string ErrorMessage) ValidateForecastRequest(ForecastRequest request)
        {
            if (request.Quantity <= 0)
                return (false, "Quantity must be greater than 0");

            if (request.Quantity > GetMaxQuantityForUnit(request.Unit))
                return (false, $"Maximum {GetMaxQuantityForUnit(request.Unit)} {request.Unit.ToString().ToLower()} allowed");

            if (request.HorizonInDays > 365)
                return (false, "Forecast horizon cannot exceed 365 days");

            return (true, string.Empty);
        }

        private static int GetMaxQuantityForUnit(ForecastUnit unit)
        {
            return unit switch
            {
                ForecastUnit.Days => 90,    
                ForecastUnit.Weeks => 52,     
                ForecastUnit.Months => 12,   
                _ => 30
            };
        }
    }

    public class Matrix
    {
        private readonly double[,] data;
        public int Rows { get; }
        public int Cols { get; }

        public Matrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            data = new double[rows, cols];
        }

        public Matrix(double[,] data)
        {
            Rows = data.GetLength(0);
            Cols = data.GetLength(1);
            this.data = new double[Rows, Cols];
            Array.Copy(data, this.data, data.Length);
        }

        public double this[int row, int col]
        {
            get => data[row, col];
            set => data[row, col] = value;
        }

        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("Matrix dimensions must match for addition");

            var result = new Matrix(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
                for (int j = 0; j < a.Cols; j++)
                    result[i, j] = a[i, j] + b[i, j];
            return result;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            if (a.Cols != b.Rows)
                throw new ArgumentException("Matrix dimensions incompatible for multiplication");

            var result = new Matrix(a.Rows, b.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < b.Cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Cols; k++)
                        sum += a[i, k] * b[k, j];
                    result[i, j] = sum;
                }
            }
            return result;
        }

        public Matrix Transpose()
        {
            var result = new Matrix(Cols, Rows);
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    result[j, i] = data[i, j];
            return result;
        }

        public Matrix Inverse()
        {
            if (Rows != Cols)
                throw new InvalidOperationException("Matrix must be square to compute inverse");

            var n = Rows;
            var augmented = new Matrix(n, 2 * n);

            // Create augmented matrix [A|I]
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    augmented[i, j] = data[i, j];
                augmented[i, i + n] = 1.0;
            }

            // Gauss-Jordan elimination with partial pivoting
            for (int i = 0; i < n; i++)
            {
                // Find pivot with better numerical stability
                int pivotRow = i;
                double maxPivot = Math.Abs(augmented[i, i]);

                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(augmented[k, i]) > maxPivot)
                    {
                        pivotRow = k;
                        maxPivot = Math.Abs(augmented[k, i]);
                    }
                }

                // Check for singular matrix with better tolerance
                if (maxPivot < 1e-12)
                {
                    // Add regularization to diagonal
                    for (int j = 0; j < n; j++)
                        augmented[j, j] += 1e-6;

                    // Retry with regularized matrix
                    maxPivot = Math.Abs(augmented[i, i]);
                    if (maxPivot < 1e-12)
                        throw new InvalidOperationException("Matrix is singular and cannot be inverted");
                }

                // Swap rows if needed
                if (pivotRow != i)
                {
                    for (int j = 0; j < 2 * n; j++)
                    {
                        (augmented[i, j], augmented[pivotRow, j]) = (augmented[pivotRow, j], augmented[i, j]);
                    }
                }

                // Scale pivot row
                double pivot = augmented[i, i];
                for (int j = 0; j < 2 * n; j++)
                    augmented[i, j] /= pivot;

                // Eliminate column
                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = augmented[k, i];
                        for (int j = 0; j < 2 * n; j++)
                            augmented[k, j] -= factor * augmented[i, j];
                    }
                }
            }

            // Extract inverse matrix
            var inverse = new Matrix(n, n);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    inverse[i, j] = augmented[i, j + n];

            return inverse;
        }

        public static Matrix Identity(int size)
        {
            var result = new Matrix(size, size);
            for (int i = 0; i < size; i++)
                result[i, i] = 1.0;
            return result;
        }
    }

    public class RBFKernel
    {
        public double LengthScale { get; set; } = 1.0;
        public double Variance { get; set; } = 1.0;

        public double Compute(double[] x1, double[] x2)
        {
            if (x1.Length != x2.Length)
                throw new ArgumentException("Input dimensions must match");

            double sumSquaredDiff = 0;
            for (int i = 0; i < x1.Length; i++)
            {
                double diff = x1[i] - x2[i];
                sumSquaredDiff += diff * diff;
            }

            // Fixed: Proper RBF kernel formula
            double result = Variance * Math.Exp(-0.5 * sumSquaredDiff / (LengthScale * LengthScale));

            // Ensure numerical stability
            return Math.Max(result, 1e-15);
        }

        public Matrix ComputeKernelMatrix(List<double[]> points1, List<double[]> points2)
        {
            var K = new Matrix(points1.Count, points2.Count);
            for (int i = 0; i < points1.Count; i++)
            {
                for (int j = 0; j < points2.Count; j++)
                {
                    K[i, j] = Compute(points1[i], points2[j]);
                }
            }
            return K;
        }
    }

    public class GaussianProcess
    {
        private readonly RBFKernel kernel;
        private readonly double noiseVariance;
        private List<double[]> trainingInputs;
        private List<double> trainingOutputs;
        private Matrix KInverse;

        public GaussianProcess(double lengthScale = 2.0, double signalVariance = 2.0, double noiseVariance = 0.1)
        {
            kernel = new RBFKernel { LengthScale = lengthScale, Variance = signalVariance };
            this.noiseVariance = noiseVariance;
            trainingInputs = new List<double[]>();
            trainingOutputs = new List<double>();
        }

        public void Fit(List<double[]> inputs, List<double> outputs)
        {
            if (inputs.Count != outputs.Count)
                throw new ArgumentException("Input and output counts must match");

            trainingInputs = inputs.Select(x => (double[])x.Clone()).ToList();
            trainingOutputs = new List<double>(outputs);

            // Normalize outputs for better numerical stability
            var outputMean = outputs.Average();
            var outputStd = Math.Sqrt(outputs.Select(y => (y - outputMean) * (y - outputMean)).Average());

            if (outputStd < 1e-10) outputStd = 1.0;

            // Store normalization parameters (you'll need to add these fields to the class)
            // this.outputMean = outputMean;
            // this.outputStd = outputStd;

            // Compute kernel matrix K
            var K = kernel.ComputeKernelMatrix(trainingInputs, trainingInputs);

            // Add noise and regularization to diagonal
            double regularization = Math.Max(noiseVariance, 1e-6);
            for (int i = 0; i < K.Rows; i++)
                K[i, i] += regularization;

            // Compute and store K^(-1) with multiple attempts
            int maxAttempts = 3;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    KInverse = K.Inverse();
                    return; // Success
                }
                catch (Exception)
                {
                    if (attempt == maxAttempts - 1)
                        throw; // Last attempt failed

                    // Add more regularization and try again
                    regularization *= 10;
                    for (int i = 0; i < K.Rows; i++)
                        K[i, i] += regularization;
                }
            }
        }

        public (double mean, double variance) Predict(double[] testPoint)
        {
            if (trainingInputs.Count == 0)
                return (0.0, kernel.Variance);

            try
            {
                // Compute k* (covariance between test point and training points)
                var kStar = new Matrix(trainingInputs.Count, 1);
                for (int i = 0; i < trainingInputs.Count; i++)
                    kStar[i, 0] = kernel.Compute(testPoint, trainingInputs[i]);

                // Compute k** (variance at test point)
                double kStarStar = kernel.Compute(testPoint, testPoint);

                // Convert training outputs to matrix
                var y = new Matrix(trainingOutputs.Count, 1);
                for (int i = 0; i < trainingOutputs.Count; i++)
                    y[i, 0] = trainingOutputs[i];

                // Compute mean: k*^T K^(-1) y
                var meanMatrix = kStar.Transpose() * KInverse * y;
                var mean = meanMatrix[0, 0];

                // Compute variance: k** - k*^T K^(-1) k*
                var varianceMatrix = kStar.Transpose() * KInverse * kStar;
                var variance = kStarStar - varianceMatrix[0, 0];

                // Ensure variance is positive and numerically stable
                variance = Math.Max(variance, 1e-8);

                // Clamp mean to reasonable bounds
                mean = Math.Max(-100, Math.Min(100, mean));

                return (mean, variance);
            }
            catch (Exception)
            {
                // Fallback to prior if computation fails
                return (0.0, kernel.Variance);
            }
        }
    }

    public class ProperBayesianOptimization
    {
        private readonly Random random;
        private readonly List<double[]> evaluatedPoints;
        private readonly List<double> evaluatedValues;
        private readonly double[] lowerBounds;
        private readonly double[] upperBounds;
        private readonly int dimensions;
        private readonly GaussianProcess gp;

        public ProperBayesianOptimization(double[] lowerBounds, double[] upperBounds, int seed = 42)
        {
            this.random = new Random(seed);
            this.lowerBounds = (double[])lowerBounds.Clone();
            this.upperBounds = (double[])upperBounds.Clone();
            this.dimensions = lowerBounds.Length;
            this.evaluatedPoints = new List<double[]>();
            this.evaluatedValues = new List<double>();
            this.gp = new GaussianProcess(lengthScale: 1.0, signalVariance: 1.0, noiseVariance: 0.01);
        }

        public OptimizedHyperparameters OptimizeHyperparameters(
            Func<OptimizedHyperparameters, double> objectiveFunction,
            int maxIterations = 50,
            int randomInitPoints = 10)
        {
            Console.WriteLine($"Starting Bayesian Optimization with {maxIterations} iterations");

            // Phase 1: Random initialization
            for (int i = 0; i < Math.Min(randomInitPoints, maxIterations); i++)
            {
                var randomParams = SampleRandomPoint();
                var hyperparams = VectorToHyperparameters(randomParams);
                var score = objectiveFunction(hyperparams);

                evaluatedPoints.Add(randomParams);
                evaluatedValues.Add(score);

                Console.WriteLine($"Random Init {i + 1}/{randomInitPoints}: Score = {score:F4}");
                Console.WriteLine($"  Params: LR={hyperparams.LearningRate:F3}, Depth={hyperparams.MaxDepth}, Trees={hyperparams.NumTrees}");
            }

            // Phase 2: Bayesian optimization iterations
            for (int iter = randomInitPoints; iter < maxIterations; iter++)
            {
                // Update GP with all evaluated points
                gp.Fit(evaluatedPoints, evaluatedValues);

                // Find next point using Expected Improvement
                var nextPoint = OptimizeAcquisitionFunction();
                var hyperparams = VectorToHyperparameters(nextPoint);
                var score = objectiveFunction(hyperparams);

                evaluatedPoints.Add(nextPoint);
                evaluatedValues.Add(score);

                var bestSoFar = evaluatedValues.Min();
                Console.WriteLine($"BO Iter {iter - randomInitPoints + 1}/{maxIterations - randomInitPoints}: Score = {score:F4}, Best = {bestSoFar:F4}");
                Console.WriteLine($"  Params: LR={hyperparams.LearningRate:F3}, Depth={hyperparams.MaxDepth}, Trees={hyperparams.NumTrees}");
            }

            // Return best hyperparameters found
            var bestIndex = evaluatedValues.IndexOf(evaluatedValues.Min());
            var bestParams = VectorToHyperparameters(evaluatedPoints[bestIndex]);

            Console.WriteLine($"\nOptimization Complete! Best Score: {evaluatedValues[bestIndex]:F4}");
            Console.WriteLine($"Best Parameters:");
            Console.WriteLine($"  Learning Rate: {bestParams.LearningRate:F4}");
            Console.WriteLine($"  Max Depth: {bestParams.MaxDepth}");
            Console.WriteLine($"  Num Trees: {bestParams.NumTrees}");
            Console.WriteLine($"  Subsample: {bestParams.Subsample:F3}");
            Console.WriteLine($"  ColSample ByTree: {bestParams.ColsampleBytree:F3}");

            return bestParams;
        }

        private double[] SampleRandomPoint()
        {
            var point = new double[dimensions];
            for (int i = 0; i < dimensions; i++)
            {
                point[i] = lowerBounds[i] + random.NextDouble() * (upperBounds[i] - lowerBounds[i]);
            }
            return point;
        }

        private double[] OptimizeAcquisitionFunction()
        {
            const int candidatePoints = 2000;
            var bestPoint = SampleRandomPoint();
            var bestAcquisition = double.MinValue;

            // Random search for acquisition function optimization
            for (int i = 0; i < candidatePoints; i++)
            {
                var candidate = SampleRandomPoint();
                var acquisitionValue = ExpectedImprovement(candidate);

                if (acquisitionValue > bestAcquisition)
                {
                    bestAcquisition = acquisitionValue;
                    bestPoint = candidate;
                }
            }

            // Local optimization around best point found
            bestPoint = LocalOptimizeAcquisition(bestPoint, 100);

            return bestPoint;
        }

        private double[] LocalOptimizeAcquisition(double[] startPoint, int iterations)
        {
            var currentPoint = (double[])startPoint.Clone();
            var currentValue = ExpectedImprovement(currentPoint);

            double stepSize = 0.1;

            for (int iter = 0; iter < iterations; iter++)
            {
                var improved = false;

                for (int dim = 0; dim < dimensions; dim++)
                {
                    // Try positive step
                    var testPoint = (double[])currentPoint.Clone();
                    testPoint[dim] = Math.Min(upperBounds[dim], testPoint[dim] + stepSize * (upperBounds[dim] - lowerBounds[dim]));
                    var testValue = ExpectedImprovement(testPoint);

                    if (testValue > currentValue)
                    {
                        currentPoint = testPoint;
                        currentValue = testValue;
                        improved = true;
                        continue;
                    }

                    // Try negative step
                    testPoint = (double[])currentPoint.Clone();
                    testPoint[dim] = Math.Max(lowerBounds[dim], testPoint[dim] - stepSize * (upperBounds[dim] - lowerBounds[dim]));
                    testValue = ExpectedImprovement(testPoint);

                    if (testValue > currentValue)
                    {
                        currentPoint = testPoint;
                        currentValue = testValue;
                        improved = true;
                    }
                }

                if (!improved)
                    stepSize *= 0.9; // Reduce step size if no improvement

                if (stepSize < 1e-6)
                    break;
            }

            return currentPoint;
        }

        private double ExpectedImprovement(double[] point, double xi = 0.1) // Increased xi
        {
            if (evaluatedPoints.Count == 0)
                return 1.0;

            try
            {
                var (mean, variance) = gp.Predict(point);
                var sigma = Math.Sqrt(Math.Max(variance, 1e-10)); // Ensure positive variance

                if (sigma < 1e-6)
                    return 1e-6; // Small positive value instead of 0

                var bestValue = evaluatedValues.Min(); // Assuming minimization
                var improvement = bestValue - mean - xi;
                var z = improvement / sigma;

                // Clamp z to reasonable bounds to avoid numerical issues
                z = Math.Max(-10, Math.Min(10, z));

                var phi = NormalCDF(z);
                var phiDensity = NormalPDF(z);

                var ei = improvement * phi + sigma * phiDensity;
                return Math.Max(ei, 1e-10); // Ensure positive EI
            }
            catch (Exception)
            {
                return random.NextDouble(); // Random exploration if prediction fails
            }
        }

        private static double NormalCDF(double x)
        {
            // Approximation of normal CDF using error function
            return 0.5 * (1 + Erf(x / Math.Sqrt(2)));
        }

        private static double NormalPDF(double x)
        {
            return Math.Exp(-0.5 * x * x) / Math.Sqrt(2 * Math.PI);
        }

        private static double Erf(double x)
        {
            // Approximation of error function
            const double a1 = 0.254829592;
            const double a2 = -0.284496736;
            const double a3 = 1.421413741;
            const double a4 = -1.453152027;
            const double a5 = 1.061405429;
            const double p = 0.3275911;

            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        private OptimizedHyperparameters VectorToHyperparameters(double[] vector)
        {
            return new OptimizedHyperparameters
            {
                LearningRate = vector[0],
                MaxDepth = (int)Math.Round(vector[1]),
                NumTrees = (int)Math.Round(vector[2]),
                Subsample = vector[3],
                ColsampleBytree = vector[4],
                MinChildWeight = vector[5],
                RegAlpha = vector[6],
                RegLambda = vector[7]
            };
        }
    }

    public static class ProperBayesianOptimizationExtensions
    {
        public static OptimizedHyperparameters RunProperBayesianOptimization(
            this ImprovedDemandForecaster forecaster,
            List<EnhancedXgBoostInput> trainData,
            List<EnhancedXgBoostInput> validationData,
            int iterations = 50)
        {
            Console.WriteLine("Using Proper Bayesian Optimization with Gaussian Process");

            // FIXED: Better bounds for hyperparameters
            var lowerBounds = new double[] { 0.01, 3, 50, 0.5, 0.5, 1, 0, 0.1 };
            var upperBounds = new double[] { 0.5, 15, 1000, 1.0, 1.0, 10, 1.0, 10.0 };

            // FIXED: Better GP hyperparameters
            var bayesOpt = new ProperBayesianOptimization(lowerBounds, upperBounds);

            // Pre-allocate arrays for validation
            var validationActualArray = new float[validationData.Count];
            for (int i = 0; i < validationData.Count; i++)
            {
                validationActualArray[i] = validationData[i].Demand;
            }

            // Define objective function with better error handling
            double ObjectiveFunction(OptimizedHyperparameters hyperparams)
            {
                try
                {
                    var model = CreateXgBoostModel(trainData, hyperparams);
                    var predictions = PredictWithXgBoostModel(model, validationData);

                    if (predictions.Count != validationData.Count)
                    {
                        Console.WriteLine($"Warning: Prediction count mismatch");
                        return double.MaxValue;
                    }

                    var predictionsSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(predictions);
                    var validationActualSpan = validationActualArray.AsSpan();

                    var smape = CalculateSMAPEStatic(predictionsSpan, validationActualSpan);

                    // Add penalty for extreme hyperparameters
                    double penalty = 0;
                    if (hyperparams.LearningRate > 0.3) penalty += 5;
                    if (hyperparams.MaxDepth > 12) penalty += 10;
                    if (hyperparams.NumTrees > 800) penalty += 5;

                    return smape + penalty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in objective function: {ex.Message}");
                    return 1000.0; // High penalty instead of MaxValue
                }
            }

            // FIXED: Better initialization ratio
            int randomInitPoints = Math.Max(10, iterations / 5);

            return bayesOpt.OptimizeHyperparameters(ObjectiveFunction, iterations, randomInitPoints);
        }
        // Keep the existing helper methods
        private static XGBRegressor CreateXgBoostModel(List<EnhancedXgBoostInput> trainData, OptimizedHyperparameters hyperparams)
        {
            float[][] dataTrain = trainData.Select(d => new float[]
            {
                d.Lag1, d.Lag2, d.Lag3, d.Lag7, d.MovingAvg3, d.MovingAvg7, d.MovingAvg14, d.MovingAvg21,
                d.ExponentialSmoothing, d.Volatility, d.Trend, d.DayOfWeek, d.Month, d.Quarter, d.DayOfYear,
                d.WeekOfYear, d.Price, d.PriceChange, d.StockLevels, d.StockRatio, d.ProcurementLeadTime,
                d.ManufacturingLeadTime, d.ProphetForecast, d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd,
                d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
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

        private static List<float> PredictWithXgBoostModel(XGBRegressor regressor, List<EnhancedXgBoostInput> inputs)
        {
            float[][] data = inputs.Select(d => new float[]
            {
                d.Lag1, d.Lag2, d.Lag3, d.Lag7, d.MovingAvg3, d.MovingAvg7, d.MovingAvg14, d.MovingAvg21,
                d.ExponentialSmoothing, d.Volatility, d.Trend, d.DayOfWeek, d.Month, d.Quarter, d.DayOfYear,
                d.WeekOfYear, d.Price, d.PriceChange, d.StockLevels, d.StockRatio, d.ProcurementLeadTime,
                d.ManufacturingLeadTime, d.ProphetForecast, d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd,
                d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
            }).ToArray();

            float[] predictions = regressor.Predict(data);
            return predictions.Select(p => Math.Max(0, p)).ToList();
        }

        private static double CalculateSMAPEStatic(ReadOnlySpan<float> predictions, ReadOnlySpan<float> actual)
        {
            if (predictions.Length != actual.Length)
                throw new ArgumentException("Predictions and actual values must have the same length");

            double sum = 0;
            for (int i = 0; i < predictions.Length; i++)
            {
                float a = actual[i];
                float p = predictions[i];
                sum += 2 * Math.Abs(a - p) / (Math.Abs(a) + Math.Abs(p) + 1e-10);
            }
            return (sum / predictions.Length) * 100;
        }
    }


    public partial class ImprovedDemandForecaster
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
                var results = new ConcurrentBag<ForecastResult>();

                Parallel.ForEach(groupedData, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, (group) =>
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
                            ProcurementLeadTime = d.ProcurementLeadTime,
                            ManufacturingLeadTime = d.ManufacturingLeadTime,
                            DayOfWeek = d.DayOfWeek,
                            Month = d.Month,
                            Quarter = d.Quarter,
                            DayOfYear = d.Date.DayOfYear,
                            WeekOfYear = GetWeekOfYear(d.Date),
                            IsWeekend = d.IsWeekend,
                            Year = d.Year,
                            ProphetSeasonalSpike = 1.0f // Will be filled by Prophet
                        }).ToList();

                    if (productData.Count >= 21)
                    {
                        var result = ForecastDemand(productData, group.Key, horizon);
                        results.Add(result);
                    }
                    else
                    {
                        var fallbackResult = CreateFallbackForecast(group.Key, productData, horizon);
                        results.Add(fallbackResult);
                    }
                });

                return results.ToList();
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
        private static List<EnhancedXgBoostInput> PrepareEnhancedXgBoostDataWithProphetSpikes(
            List<DemandData> fullData, int startIndex, int count)
        {
            var xgBoostData = new List<EnhancedXgBoostInput>();

            for (int i = startIndex; i < startIndex + count && i < fullData.Count; i++)
            {
                if (i < 21) continue;

                var demand = fullData[i].Demand;

                // Get recent demands as span for efficient calculations
                var recentDemandsArray = new float[21];
                for (int j = 0; j < 21; j++)
                {
                    recentDemandsArray[j] = fullData[i - 21 + j].Demand;
                }
                var recentDemandsSpan = recentDemandsArray.AsSpan();

                // Calculate technical features
                var movingAvg3 = CalculateMovingAverage(recentDemandsSpan, 3);
                var movingAvg7 = CalculateMovingAverage(recentDemandsSpan, 7);
                var movingAvg14 = CalculateMovingAverage(recentDemandsSpan, 14);
                var movingAvg21 = CalculateMovingAverage(recentDemandsSpan, 21);

                var exponentialSmoothing = CalculateExponentialSmoothing(recentDemandsSpan, 0.3f);
                var volatility = CalculateStandardDeviation(recentDemandsSpan.Slice(14));

                var trend = recentDemandsSpan.Length >= 7 ?
                    (CalculateMovingAverage(recentDemandsSpan.Slice(18), 3) - CalculateMovingAverage(recentDemandsSpan.Slice(0, 3), 3)) / 18f : 0;

                // KEY: Use Prophet seasonal spike as a feature
                var prophetSeasonalSpike = fullData[i].ProphetSeasonalSpike;

                var rollingStd7 = CalculateStandardDeviation(recentDemandsSpan.Slice(14));
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
                    ProcurementLeadTime = fullData[i].ProcurementLeadTime,
                    ManufacturingLeadTime = fullData[i].ManufacturingLeadTime,
                    ProphetForecast = fullData[i].ProphetSeasonalSpike,
                    IsWeekend = fullData[i].IsWeekend,
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


        private static (Dictionary<DateTime, float> historical, List<float> future) DetectSeasonalSpikesWithProphetEnhanced(
                    List<DemandData> data, int horizon)
        {
            const int maxRetries = 2;

            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    var historicalData = data.Select(d => new
                    {
                        ds = d.Date.ToString("yyyy-MM-dd"),
                        y = d.Demand
                    }).ToList();

                    var inputData = new
                    {
                        historical_data = historicalData,
                        horizon = horizon
                    };
                    var jsonInput = JsonConvert.SerializeObject(inputData);

                    var tempFilePath = Path.GetTempFileName();
                    File.WriteAllText(tempFilePath, jsonInput);

                    var pythonPath = @"C:\Users\yosri\AppData\Local\Microsoft\WindowsApps\python.exe";
                    var scriptPath =
                        @"C:\Users\yosri\Desktop\projects for me\intership 4éme\MLINTERNSHIP\MLINTERNSHIP\prophet_seasonal_detector.py";

                    using (var process = new Process
                           {
                               StartInfo = new ProcessStartInfo
                               {
                                   FileName = pythonPath,
                                   Arguments = $"\"{scriptPath}\" \"{tempFilePath}\"",
                                   RedirectStandardOutput = true,
                                   RedirectStandardError = true,
                                   UseShellExecute = false,
                                   CreateNoWindow = true
                               }
                           })
                    {
                        process.Start();
                        if (!process.WaitForExit(30000)) // 30 second timeout
                        {
                            process.Kill();
                            if (retry == maxRetries)
                                return CreateFallbackSeasonalData(data, horizon);
                            continue;
                        }

                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();
                        File.Delete(tempFilePath);

                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            var result = JsonConvert.DeserializeObject<dynamic>(output);
                            var historical = new Dictionary<DateTime, float>();
                            if (result.historical_seasonal_spikes != null)
                            {
                                foreach (var kvp in result.historical_seasonal_spikes)
                                {
                                    if (DateTime.TryParseExact(kvp.Name, "yyyy-MM-dd",
                                            CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                                    {
                                        historical[date] = (float)kvp.Value;
                                    }
                                }
                            }

                            var future = new List<float>();
                            if (result.future_seasonal_spikes != null)
                            {
                                foreach (var spike in result.future_seasonal_spikes)
                                {
                                    future.Add((float)spike);
                                }
                            }

                            return (historical, future);
                        }
                        else if (retry == maxRetries)
                        {
                            Console.WriteLine($"Prophet error after {maxRetries + 1} attempts: {error}");
                        }
                        else
                        {
                            Console.WriteLine($"Prophet error: {error}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (retry == maxRetries)
                    {
                        Console.WriteLine($"Prophet seasonal detection failed after {maxRetries + 1} attempts: {ex.Message}");
                        return CreateFallbackSeasonalData(data, horizon);
                    }

                    // Wait before retry
                    Thread.Sleep(1000);
                }
            }

            return CreateFallbackSeasonalData(data, horizon);
            
        }
        private static (Dictionary<DateTime, float> historical, List<float> future) CreateFallbackSeasonalData(
            List<DemandData> data, int horizon)
        {
            var historical = new Dictionary<DateTime, float>();
            var future = new List<float>();

            // Create fallback historical patterns
            foreach (var item in data)
            {
                float spike = 1.0f;

                // Weekend effect
                if (item.Date.DayOfWeek == DayOfWeek.Saturday || item.Date.DayOfWeek == DayOfWeek.Sunday)
                    spike *= 1.2f;

                // Month-end effect
                if (item.Date.Day >= 28)
                    spike *= 1.1f;

                // Holiday season effect
                if (item.Date.Month >= 11)
                    spike *= 1.3f;

                historical[item.Date] = Math.Max(0.5f, Math.Min(3.0f, spike));
            }

            // Create fallback future patterns
            var lastDate = data.Last().Date;
            for (int i = 1; i <= horizon; i++)
            {
                var futureDate = lastDate.AddDays(i);
                float spike = 1.0f;

                if (futureDate.DayOfWeek == DayOfWeek.Saturday || futureDate.DayOfWeek == DayOfWeek.Sunday)
                    spike *= 1.2f;
                if (futureDate.Day >= 28)
                    spike *= 1.1f;
                if (futureDate.Month >= 11)
                    spike *= 1.3f;

                future.Add(Math.Max(0.5f, Math.Min(3.0f, spike)));
            }

            return (historical, future);
        }
        private static float? GetInterpolatedSeasonalSpike(Dictionary<DateTime, float> spikes, DateTime targetDate)
        {
            if (spikes.Count == 0) return null;

            // Find the closest dates
            var before = spikes.Keys.Where(d => d < targetDate).OrderByDescending(d => d).FirstOrDefault();
            var after = spikes.Keys.Where(d => d > targetDate).OrderBy(d => d).FirstOrDefault();

            if (before != default && after != default)
            {
                // Linear interpolation
                var totalDays = (after - before).TotalDays;
                var targetDays = (targetDate - before).TotalDays;
                var ratio = totalDays > 0 ? targetDays / totalDays : 0;

                return spikes[before] + (float)(ratio * (spikes[after] - spikes[before]));
            }
            else if (before != default)
            {
                return spikes[before];
            }
            else if (after != default)
            {
                return spikes[after];
            }

            return null;
        }
        public ForecastResult ForecastDemand(IEnumerable<DemandData> data, string productId, int horizon = 7)
        {
            var productData = data.OrderBy(d => d.Date).ToList();
            if (productData.Count < 21)
                throw new ArgumentException($"Insufficient data for product {productId}.");

            Console.WriteLine($"\nProcessing {productId} with {productData.Count} data points");

            // Step 1: Get both historical and future Prophet seasonal patterns in one call
            var (historicalSpikes, futureSpikes) = DetectSeasonalSpikesWithProphetEnhanced(productData, horizon);

            // Step 2: Apply Prophet spikes to historical data
            for (int i = 0; i < productData.Count; i++)
            {
                if (historicalSpikes.ContainsKey(productData[i].Date))
                {
                    productData[i].ProphetSeasonalSpike = historicalSpikes[productData[i].Date];
                }
                else
                {
                    var nearbySpike = GetInterpolatedSeasonalSpike(historicalSpikes, productData[i].Date);
                    productData[i].ProphetSeasonalSpike = nearbySpike ?? CalculateFallbackSeasonality(productData[i].Date);
                }
            }

            // Step 3: Preprocess data with transformation
            var (processedData, transformationInfo) = EnhancedPreprocessDataWithTransformation(productData);

            // Step 4: Prepare training/validation split
            var trainRatio = Math.Max(0.7, Math.Min(0.9, 1.0 - (14.0 / processedData.Count)));
            var trainSize = (int)(processedData.Count * trainRatio);
            var validationSize = processedData.Count - trainSize;

            Console.WriteLine($"  Training: {trainSize} samples, Validation: {validationSize} samples");

            // Step 5: Prepare XGBoost data with Prophet seasonal features
            var xgBoostTrainData = PrepareEnhancedXgBoostDataWithProphetSpikes(processedData, 21, trainSize - 21);
            var xgBoostValidationData = PrepareEnhancedXgBoostDataWithProphetSpikes(processedData, trainSize, validationSize);

            if (xgBoostTrainData.Count == 0 || xgBoostValidationData.Count == 0)
            {
                Console.WriteLine($"  Warning: Insufficient XGBoost training data");
                return CreateFallbackForecast(productId, processedData, horizon);
            }

            // Step 6: Optimize hyperparameters and train
            Console.WriteLine($"  Starting Bayesian optimization...");
            var optimalParams = this.RunProperBayesianOptimization(xgBoostTrainData, xgBoostValidationData, 30); // Reduced iterations for speed

            Console.WriteLine($"  Training final XGBoost model...");
            var xgBoostModel = TrainXgBoost(xgBoostTrainData, optimalParams);

            // Step 7: Create future features using Prophet seasonal patterns
            var xgBoostFutureData = CreateEnhancedFutureFeatures(processedData, futureSpikes, horizon);

            // Step 8: Make predictions
            var validationPredictions = PredictWithXgBoost(xgBoostModel, xgBoostValidationData);
            var futurePredictions = PredictWithXgBoost(xgBoostModel, xgBoostFutureData);

            // Step 9: Evaluate performance
            var actualValidationArray = new float[validationSize];
            for (int i = 0; i < validationSize; i++)
            {
                actualValidationArray[i] = processedData[trainSize + i].Demand;
            }

            var validationPredictionsSpan = CollectionsMarshal.AsSpan(validationPredictions);
            var actualValidationSpan = actualValidationArray.AsSpan();
            var xgBoostSMAPE = CalculateSMAPE(validationPredictionsSpan, actualValidationSpan);

            var trainDataArray = new float[trainSize];
            for (int i = 0; i < trainSize; i++)
            {
                trainDataArray[i] = processedData[i].Demand;
            }
            var trainDataSpan = trainDataArray.AsSpan();

            var metrics = CalculateComprehensiveMetrics(actualValidationSpan, validationPredictionsSpan, trainDataSpan);

            // Step 10: Post-process predictions
            var revertedPredictions = InverseTransformation(futurePredictions, transformationInfo);
            var revertedConfidenceIntervals = CalculateConfidenceIntervals(revertedPredictions, metrics.RMSE);

            Console.WriteLine($"  Final SMAPE: {xgBoostSMAPE:F2}%");

            return new ForecastResult
            {
                ProductId = productId,
                Predictions = revertedPredictions,
                ProphetPredictions = revertedPredictions, // Using same predictions as we integrated Prophet features
                XgBoostPredictions = revertedPredictions,
                Metrics = metrics,
                OptimalHyperparameters = optimalParams,
                ConfidenceIntervals = revertedConfidenceIntervals,
                SelectedModel = "XGBoost with Integrated Prophet Seasonality",
                XgBoostSMAPE = xgBoostSMAPE,
                ProphetSMAPE = xgBoostSMAPE // Same model now
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
                    ProcurementLeadTime = Math.Max(1, item.ProcurementLeadTime),
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
            var seriesSpan = CollectionsMarshal.AsSpan(series);

            // Allocate arrays for transformations
            var logTransformed = new float[series.Count];
            var sqrtTransformed = new float[series.Count];
            var boxCoxTransformed = new float[series.Count];

            // Apply transformations using spans
            for (int i = 0; i < seriesSpan.Length; i++)
            {
                logTransformed[i] = MathF.Log(seriesSpan[i] + 1);
                sqrtTransformed[i] = MathF.Sqrt(seriesSpan[i]);
            }

            const float lambda = 0.5f;
            ApplyBoxCoxTransformation(seriesSpan, boxCoxTransformed, lambda);

            // Calculate skewness using spans
            var originalSkewness = CalculateSkewness(seriesSpan);
            var logSkewness = CalculateSkewness(logTransformed);
            var sqrtSkewness = CalculateSkewness(sqrtTransformed);
            var boxCoxSkewness = CalculateSkewness(boxCoxTransformed);

            var transformations = new[]
            {
                (Math.Abs(originalSkewness), original, "None", 0f),
                (Math.Abs(logSkewness), logTransformed.ToList(), "Log", 0f),
                (Math.Abs(sqrtSkewness), sqrtTransformed.ToList(), "Sqrt", 0f),
                (Math.Abs(boxCoxSkewness), boxCoxTransformed.ToList(), "BoxCox", lambda)
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
        private static float CalculateSkewness(ReadOnlySpan<float> series)
        {
            if (series.Length == 0) return 0;

            float sum = 0;
            for (int i = 0; i < series.Length; i++)
            {
                sum += series[i];
            }
            float mean = sum / series.Length;

            float sumSquaredDiff = 0;
            for (int i = 0; i < series.Length; i++)
            {
                float diff = series[i] - mean;
                sumSquaredDiff += diff * diff;
            }
            float std = MathF.Sqrt(sumSquaredDiff / series.Length);

            if (std < 1e-10) return 0;

            float sumCubedNorm = 0;
            for (int i = 0; i < series.Length; i++)
            {
                float normalizedDiff = (series[i] - mean) / std;
                sumCubedNorm += normalizedDiff * normalizedDiff * normalizedDiff;
            }

            return sumCubedNorm / series.Length;
        }
        private static void ApplyBoxCoxTransformation(ReadOnlySpan<float> input, Span<float> output, float lambda)
        {
            if (input.Length != output.Length)
                throw new ArgumentException("Input and output spans must have the same length");

            if (Math.Abs(lambda) < 1e-10)
            {
                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = MathF.Log(input[i] + 1);
                }
            }
            else
            {
                for (int i = 0; i < input.Length; i++)
                {
                    output[i] = (MathF.Pow(input[i] + 1, lambda) - 1) / lambda;
                }
            }
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
        private static float CalculateExponentialSmoothing(ReadOnlySpan<float> data, float alpha)
        {
            if (data.Length == 0) return 0;

            float smoothed = data[0];
            for (int i = 1; i < data.Length; i++)
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
        private XGBRegressor TrainXgBoost(List<EnhancedXgBoostInput> trainData, OptimizedHyperparameters hyperparams)
        {
            float[][] dataTrain = trainData.Select(d => new float[]
            {
                d.Lag1, d.Lag2, d.Lag3, d.Lag7,
                d.MovingAvg3, d.MovingAvg7, d.MovingAvg14, d.MovingAvg21,
                d.ExponentialSmoothing, d.Volatility, d.Trend,
                d.DayOfWeek, d.Month, d.Quarter, d.DayOfYear, d.WeekOfYear,
                d.Price, d.PriceChange, d.StockLevels, d.StockRatio,
                d.ProcurementLeadTime, d.ManufacturingLeadTime,
                d.ProphetForecast,
                d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd,
                d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
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
        d.WeekOfYear, d.Price, d.PriceChange, d.StockLevels, d.StockRatio, d.ProcurementLeadTime,d.ManufacturingLeadTime, d.ProphetForecast,
        d.IsWeekend, d.IsMonthEnd, d.IsQuarterEnd, d.RollingStd7, d.MovingAvgDiff, d.Lag1Diff, d.Lag7Diff
            }).ToArray();
            float[] predictions = regressor.Predict(data);
            return predictions.Select(p => Math.Max(0, p)).ToList();
        }
        private static List<EnhancedXgBoostInput> CreateEnhancedFutureFeatures(List<DemandData> historicalData, List<float> prophetSeasonalSpikes, int horizon)
        {
            var futureFeatures = new List<EnhancedXgBoostInput>();
            var lastDate = historicalData.Last().Date;
            var recentDemands = historicalData.TakeLast(21).Select(d => d.Demand).ToList();

            for (int i = 0; i < horizon; i++)
            {
                var futureDate = lastDate.AddDays(i + 1);
                var currentDemands = new List<float>(recentDemands);

                // Use Prophet-adjusted predictions for lag features
                if (i > 0)
                {
                    // Apply Prophet seasonality to previous predictions
                    var adjustedPreviousPredictions = prophetSeasonalSpikes.Take(i)
                        .Select((spike, idx) => recentDemands.Last() * spike).ToList();
                    currentDemands.AddRange(adjustedPreviousPredictions);
                }
                var movingAvg3 = currentDemands.TakeLast(3).Average();
                var movingAvg7 = currentDemands.TakeLast(7).Average();
                var movingAvg14 = currentDemands.TakeLast(14).Average();
                var movingAvg21 = currentDemands.TakeLast(21).Average();

                var currentDemandsLast21 = currentDemands.TakeLast(21).ToArray();
                var exponentialSmoothing = CalculateExponentialSmoothing(currentDemandsLast21.AsSpan(), 0.3f);

                var volatility = (float)Math.Sqrt(currentDemands.TakeLast(7).Select(d => Math.Pow(d - movingAvg7, 2)).Average());
                var trend = currentDemands.Count >= 7 ? (currentDemands.TakeLast(3).Average() - currentDemands.Take(3).Average()) / 18f : 0;
                var rollingStd7 = (float)StandardDeviation(currentDemands.TakeLast(7));
                var movingAvgDiff = movingAvg7 - movingAvg21;
                var lag1Diff = i > 0 ? prophetSeasonalSpikes[i - 1] - currentDemands.Last() : currentDemands.Last() - currentDemands[^2];
                var lag7Diff = i >= 6 ? prophetSeasonalSpikes[i - 1] - currentDemands[^7] : 0;

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
                    ProcurementLeadTime = historicalData.Last().ProcurementLeadTime,
                    ManufacturingLeadTime = historicalData.Last().ManufacturingLeadTime,
                    ProphetForecast = prophetSeasonalSpikes[i], // Changed to ProphetForecast and fixed index
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
        private static double CalculateSMAPE(ReadOnlySpan<float> predictions, ReadOnlySpan<float> actual)
        {
            if (predictions.Length != actual.Length)
                throw new ArgumentException("Predictions and actual values must have the same length");

            double sum = 0;
            for (int i = 0; i < predictions.Length; i++)
            {
                float a = actual[i];
                float p = predictions[i];
                sum += 2 * Math.Abs(a - p) / (Math.Abs(a) + Math.Abs(p) + 1e-10);
            }
            return (sum / predictions.Length) * 100;
        }
        private static EvaluationMetrics CalculateComprehensiveMetrics(ReadOnlySpan<float> actual, ReadOnlySpan<float> predicted, ReadOnlySpan<float> trainData)
        {
            if (actual.Length != predicted.Length)
                throw new ArgumentException("Actual and predicted values must have the same length");

            var n = actual.Length;

            // Calculate mean using span
            float actualSum = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                actualSum += actual[i];
            }
            float actualMean = actualSum / actual.Length;

            // Calculate metrics using spans
            float mapeSum = 0;
            float rmseSum = 0;
            float maeSum = 0;
            float ssRes = 0;
            float smapeSum = 0;
            int mapeCount = 0;

            for (int i = 0; i < actual.Length; i++)
            {
                float a = actual[i];
                float p = predicted[i];
                float error = a - p;
                float absError = Math.Abs(error);

                // MAPE calculation (avoid division by zero)
                if (a != 0)
                {
                    mapeSum += absError / Math.Abs(a);
                    mapeCount++;
                }

                // RMSE
                rmseSum += error * error;

                // MAE
                maeSum += absError;

                // SS_res for R²
                ssRes += error * error;

                // SMAPE
                smapeSum += 2 * absError / (Math.Abs(a) + Math.Abs(p) + 1e-10f);
            }

            var mape = mapeCount > 0 ? (mapeSum / mapeCount) * 100 : 0;
            var rmse = Math.Sqrt(rmseSum / actual.Length);
            var mae = maeSum / actual.Length;
            var smape = (smapeSum / actual.Length) * 100;

            // Calculate SS_tot for R²
            float ssTot = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                float diff = actual[i] - actualMean;
                ssTot += diff * diff;
            }

            var r2 = ssTot == 0 ? 1.0 : 1 - (ssRes / ssTot);

            // Calculate naive forecast error for MASE
            float naiveForecastErrorSum = 0;
            for (int i = 1; i < trainData.Length; i++)
            {
                naiveForecastErrorSum += Math.Abs(trainData[i] - trainData[i - 1]);
            }
            var naiveForecastError = naiveForecastErrorSum / (trainData.Length - 1);
            var mase = naiveForecastError > 0 ? mae / naiveForecastError : 0;

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
        private static float CalculateMovingAverage(ReadOnlySpan<float> data, int windowSize)
        {
            if (data.Length == 0) return 0;

            int actualWindow = Math.Min(windowSize, data.Length);
            float sum = 0;

            for (int i = data.Length - actualWindow; i < data.Length; i++)
            {
                sum += data[i];
            }

            return sum / actualWindow;
        }
        private static float CalculateFallbackSeasonality(DateTime date)
        {
            float spike = 1.0f;

            // Weekend effect
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                spike *= 1.2f;

            // Month-end effect  
            if (date.Day >= 28)
                spike *= 1.1f;

            // Holiday season effect
            if (date.Month >= 11)
                spike *= 1.3f;

            return Math.Max(0.5f, Math.Min(3.0f, spike));
        }
        private static float CalculateStandardDeviation(ReadOnlySpan<float> values)
        {
            if (values.Length == 0) return 0;

            float sum = 0;
            for (int i = 0; i < values.Length; i++)
            {
                sum += values[i];
            }
            float mean = sum / values.Length;

            float sumSquaredDiff = 0;
            for (int i = 0; i < values.Length; i++)
            {
                float diff = values[i] - mean;
                sumSquaredDiff += diff * diff;
            }

            return MathF.Sqrt(sumSquaredDiff / values.Length);
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