using FluentAssertions;
using MLINTERNSHIP;
using Xunit;

namespace FrontEndForecasting1.Tests
{
    public class ForecastingAlgorithmTests
    {
        private readonly ImprovedDemandForecaster _forecaster;

        public ForecastingAlgorithmTests()
        {
            _forecaster = new ImprovedDemandForecaster();
        }

        [Fact]
        public void LoadCsvData_WithValidData_ShouldReturnList()
        {
            // Arrange
            var csvContent = "Date,SKU,Product type,Price,Availability,Number of products sold,Revenue generated,Customer demographics,Stock levels,Procurement lead time,Shipping times,Shipping carriers,Shipping costs,Supplier name,Location,Manufacturing lead time,Manufacturing costs,is_weekend,day_of_week,month,quarter,year\n" +
                           "2023-01-01,SKU001,Electronics,100.00,In Stock,50,5000.00,Young Adults,200,5,3,FedEx,15.00,Supplier A,New York,10,500.00,0,1,1,1,2023\n" +
                           "2023-01-02,SKU001,Electronics,100.00,In Stock,45,4500.00,Young Adults,180,5,3,FedEx,15.00,Supplier A,New York,10,500.00,0,2,1,1,2023";

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));

            // Act
            var result = ImprovedDemandForecaster.LoadCsvData(stream);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].SKU.Should().Be("SKU001");
            result[0].Date.Should().Be(new DateTime(2023, 1, 1));
        }

        [Fact]
        public void ValidateForecastRequest_WithValidRequest_ShouldReturnTrue()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Days,
                Quantity = 7
            };

            // Act
            var (isValid, errorMessage) = ImprovedDemandForecaster.ValidateForecastRequest(request);

            // Assert
            isValid.Should().BeTrue();
            errorMessage.Should().BeNullOrEmpty();
        }

        [Fact]
        public void ValidateForecastRequest_WithInvalidQuantity_ShouldReturnFalse()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Days,
                Quantity = 0
            };

            // Act
            var (isValid, errorMessage) = ImprovedDemandForecaster.ValidateForecastRequest(request);

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ValidateForecastRequest_WithExcessiveQuantity_ShouldReturnFalse()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Days,
                Quantity = 1000
            };

            // Act
            var (isValid, errorMessage) = ImprovedDemandForecaster.ValidateForecastRequest(request);

            // Assert
            isValid.Should().BeFalse();
            errorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void ForecastRequest_GetDisplayText_ShouldReturnCorrectFormat()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Days,
                Quantity = 7
            };

            // Act
            var displayText = request.GetDisplayText();

            // Assert
            displayText.Should().Be("7 days");
        }

        [Fact]
        public void ForecastRequest_GetDisplayText_WithWeeks_ShouldReturnCorrectFormat()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Weeks,
                Quantity = 4
            };

            // Act
            var displayText = request.GetDisplayText();

            // Assert
            displayText.Should().Be("4 weeks (28 days)");
        }

        [Fact]
        public void ForecastRequest_HorizonInDays_ShouldCalculateCorrectly()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Days,
                Quantity = 7
            };

            // Act
            var horizon = request.HorizonInDays;

            // Assert
            horizon.Should().Be(7);
        }

        [Fact]
        public void ForecastRequest_HorizonInDays_WithWeeks_ShouldCalculateCorrectly()
        {
            // Arrange
            var request = new ForecastRequest
            {
                Unit = ForecastUnit.Weeks,
                Quantity = 2
            };

            // Act
            var horizon = request.HorizonInDays;

            // Assert
            horizon.Should().Be(14);
        }

        [Fact]
        public void EnhancedForecastResult_ShouldInheritFromForecastResult()
        {
            // Arrange & Act
            var enhancedResult = new EnhancedForecastResult();

            // Assert
            enhancedResult.Should().BeAssignableTo<ForecastResult>();
        }

        [Fact]
        public void EnhancedForecastResult_ShouldHaveRequiredProperties()
        {
            // Arrange
            var enhancedResult = new EnhancedForecastResult();

            // Act & Assert
            enhancedResult.Request.Should().NotBeNull();
            enhancedResult.ForecastDates.Should().NotBeNull();
            enhancedResult.DetailedForecasts.Should().NotBeNull();
            enhancedResult.AggregatedPredictions.Should().NotBeNull();
            enhancedResult.AggregatedConfidenceIntervals.Should().NotBeNull();
            enhancedResult.AggregatedLabels.Should().NotBeNull();
        }
    }
}
