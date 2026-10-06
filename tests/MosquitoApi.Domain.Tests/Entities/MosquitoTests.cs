using FluentAssertions;
using AutoFixture;
using AutoFixture.Xunit2;
using MosquitoApi.Domain.Entities;
using MosquitoApi.Domain.Enums;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Domain.Tests.Entities;

public class MosquitoTests
{
    private readonly IFixture _fixture;

    public MosquitoTests()
    {
        _fixture = new Fixture();
        _fixture.Customize<Location>(composer => 
            composer.FromFactory(() => new Location(
                _fixture.Create<double>() % 180 - 90, // Latitude -90 to 90
                _fixture.Create<double>() % 360 - 180, // Longitude -180 to 180
                _fixture.Create<string>())));
    }

    [Fact]
    public void Mosquito_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var mosquito = new Mosquito();

        // Assert
        mosquito.Id.Should().NotBeEmpty();
        mosquito.Name.Should().BeEmpty();
        mosquito.Species.Should().Be(default);
        mosquito.Size.Should().Be(default);
        mosquito.IsDeleted.Should().BeFalse();
        mosquito.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [AutoData]
    public void Mosquito_WithValidData_ShouldPassValidation(string name, double lengthMm, Location location)
    {
        // Arrange
        var mosquito = new Mosquito
        {
            Name = name[..Math.Min(name.Length, 200)], // Ensure within length limit
            LengthMm = Math.Abs(lengthMm) % 50 + 0.1, // Ensure positive and within limit
            Location = location,
            ObservedAt = DateTime.UtcNow.AddDays(-1),
            Species = MosquitoSpecies.AedesAegypti,
            Size = MosquitoSize.Medium
        };

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("", "Mosquito name is required")]
    [InlineData(null!, "Mosquito name is required")]
    public void Mosquito_WithInvalidName_ShouldFailValidation(string? name, string expectedError)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, name)
            .With(x => x.LengthMm, 5.0)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(0, "Length must be greater than 0 mm")]
    [InlineData(-1, "Length must be greater than 0 mm")]
    [InlineData(51, "Length cannot exceed 50 mm (unrealistic for mosquitoes)")]
    public void Mosquito_WithInvalidLength_ShouldFailValidation(double length, string expectedError)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.LengthMm, length)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain(expectedError);
    }

    [Fact]
    public void Mosquito_WithNullLocation_ShouldFailValidation()
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.LengthMm, 5.0)
            .With(x => x.Location, (Location)null!)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain("Location is required");
    }

    [Fact]
    public void Mosquito_WithFutureObservationDate_ShouldFailValidation()
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.LengthMm, 5.0)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain("Observation date cannot be in the future");
    }

    [Theory]
    [InlineData(-60, "Temperature must be between -50°C and 60°C")]
    [InlineData(70, "Temperature must be between -50°C and 60°C")]
    public void Mosquito_WithInvalidTemperature_ShouldFailValidation(double temperature, string expectedError)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.LengthMm, 5.0)
            .With(x => x.TemperatureCelsius, temperature)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(-10, "Humidity must be between 0% and 100%")]
    [InlineData(110, "Humidity must be between 0% and 100%")]
    public void Mosquito_WithInvalidHumidity_ShouldFailValidation(double humidity, string expectedError)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.LengthMm, 5.0)
            .With(x => x.HumidityPercent, humidity)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(20.0, 70.0, true)]
    [InlineData(10.0, 50.0, false)]
    [InlineData(40.0, 30.0, false)]
    [InlineData(null, null, true)]
    public void IsLikelyActive_ShouldReturnCorrectValue(double? temperature, double? humidity, bool expected)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.TemperatureCelsius, temperature)
            .With(x => x.HumidityPercent, humidity)
            .Create();

        // Act
        var result = mosquito.IsLikelyActive();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MosquitoSpecies.AedesAegypti, true, MosquitoSize.Large, "High Risk - Disease vector in favorable conditions")]
    [InlineData(MosquitoSpecies.CulexPipiens, false, MosquitoSize.Small, "Low Risk - Standard precautions recommended")]
    [InlineData(MosquitoSpecies.AnophelesGambiae, true, MosquitoSize.Medium, "Moderate-High Risk - Disease vector present")]
    public void GetRiskAssessment_ShouldReturnCorrectRiskLevel(MosquitoSpecies species, bool isDiseaseVector, MosquitoSize size, string expectedRisk)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Species, species)
            .With(x => x.IsDiseaseVector, isDiseaseVector)
            .With(x => x.Size, size)
            .With(x => x.TemperatureCelsius, 25.0) // Favorable conditions
            .With(x => x.HumidityPercent, 70.0)
            .Create();

        // Act
        var riskAssessment = mosquito.GetRiskAssessment();

        // Assert
        riskAssessment.Should().Be(expectedRisk);
    }

    [Theory]
    [InlineData(MosquitoSize.Small, 5.0, "Small mosquitoes should be less than 4mm in length")]
    [InlineData(MosquitoSize.Medium, 2.0, "Medium mosquitoes should be between 4-6mm in length")]
    [InlineData(MosquitoSize.Medium, 8.0, "Medium mosquitoes should be between 4-6mm in length")]
    [InlineData(MosquitoSize.Large, 5.0, "Large mosquitoes should be greater than 6mm in length")]
    public void Validate_WithInconsistentSizeAndLength_ShouldReturnError(MosquitoSize size, double length, string expectedError)
    {
        // Arrange
        var mosquito = _fixture.Build<Mosquito>()
            .With(x => x.Name, "Test Mosquito")
            .With(x => x.Size, size)
            .With(x => x.LengthMm, length)
            .With(x => x.ObservedAt, DateTime.UtcNow.AddDays(-1))
            .Create();

        // Act
        var validationErrors = mosquito.Validate();

        // Assert
        validationErrors.Should().Contain(expectedError);
    }
}