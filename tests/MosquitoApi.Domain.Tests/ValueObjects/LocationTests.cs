using FluentAssertions;
using MosquitoApi.Domain.ValueObjects;

namespace MosquitoApi.Domain.Tests.ValueObjects;

public class LocationTests
{
    [Theory]
    [InlineData(40.7128, -74.0060, "New York", "New York", "USA")]
    [InlineData(0.0, 0.0, null, null, null)]
    [InlineData(90.0, 180.0, "", "", "")]
    public void Location_WithValidCoordinates_ShouldCreateSuccessfully(double latitude, double longitude, string? address, string? city, string? country)
    {
        // Act
        var location = new Location(latitude, longitude, address, city, country);

        // Assert
        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
        location.Address.Should().Be(address?.Trim());
        location.City.Should().Be(city?.Trim());
        location.Country.Should().Be(country?.Trim());
    }

    [Theory]
    [InlineData(-91.0, 0.0)]
    [InlineData(91.0, 0.0)]
    public void Location_WithInvalidLatitude_ShouldThrowException(double latitude, double longitude)
    {
        // Act & Assert
        var action = () => new Location(latitude, longitude);
        
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("latitude");
    }

    [Theory]
    [InlineData(0.0, -181.0)]
    [InlineData(0.0, 181.0)]
    public void Location_WithInvalidLongitude_ShouldThrowException(double latitude, double longitude)
    {
        // Act & Assert
        var action = () => new Location(latitude, longitude);
        
        action.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("longitude");
    }

    [Fact]
    public void DistanceTo_WithSameLocation_ShouldReturnZero()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060, "New York");
        var location2 = new Location(40.7128, -74.0060, "New York");

        // Act
        var distance = location1.DistanceTo(location2);

        // Assert
        distance.Should().BeApproximately(0, 0.001);
    }

    [Fact]
    public void DistanceTo_WithKnownLocations_ShouldReturnCorrectDistance()
    {
        // Arrange - New York to Los Angeles
        var newYork = new Location(40.7128, -74.0060, "New York");
        var losAngeles = new Location(34.0522, -118.2437, "Los Angeles");

        // Act
        var distance = newYork.DistanceTo(losAngeles);

        // Assert - Approximate distance between NY and LA is ~3944 km
        distance.Should().BeApproximately(3944, 50);
    }

    [Fact]
    public void DistanceTo_WithNullLocation_ShouldThrowException()
    {
        // Arrange
        var location = new Location(40.7128, -74.0060);

        // Act & Assert
        var action = () => location.DistanceTo(null!);
        
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(40.712800, -74.006000, null, null, null, "40.712800, -74.006000")]
    [InlineData(40.712800, -74.006000, "Times Square", null, null, "40.712800, -74.006000 - Times Square")]
    [InlineData(40.712800, -74.006000, "Times Square", "New York", null, "40.712800, -74.006000 - Times Square, New York")]
    [InlineData(40.712800, -74.006000, "Times Square", "New York", "USA", "40.712800, -74.006000 - Times Square, New York, USA")]
    [InlineData(40.712800, -74.006000, null, "New York", "USA", "40.712800, -74.006000, New York, USA")]
    public void ToString_ShouldFormatCorrectly(double lat, double lon, string? address, string? city, string? country, string expected)
    {
        // Arrange
        var location = new Location(lat, lon, address, city, country);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Location_WithWhitespaceInFields_ShouldTrimValues()
    {
        // Arrange & Act
        var location = new Location(40.7128, -74.0060, "  New York  ", "  NYC  ", "  USA  ");

        // Assert
        location.Address.Should().Be("New York");
        location.City.Should().Be("NYC");
        location.Country.Should().Be("USA");
    }

    [Fact]
    public void Location_EqualityTest_ShouldWorkCorrectly()
    {
        // Arrange
        var location1 = new Location(40.7128, -74.0060, "New York", "NYC", "USA");
        var location2 = new Location(40.7128, -74.0060, "New York", "NYC", "USA");
        var location3 = new Location(34.0522, -118.2437, "Los Angeles", "LA", "USA");

        // Assert
        location1.Should().Be(location2);
        location1.Should().NotBe(location3);
        location1.GetHashCode().Should().Be(location2.GetHashCode());
        location1.GetHashCode().Should().NotBe(location3.GetHashCode());
    }

    [Theory]
    [InlineData(-90.0, -180.0)] // Extreme valid values
    [InlineData(90.0, 180.0)]   // Extreme valid values
    [InlineData(0.0, 0.0)]      // Equator and Prime Meridian
    public void Location_WithBoundaryValues_ShouldBeValid(double latitude, double longitude)
    {
        // Act & Assert
        var action = () => new Location(latitude, longitude);
        
        action.Should().NotThrow();
    }
}