using SharedComponents.Services;
using Xunit;

namespace SharedComponents.Tests;

public class MetricsGeneratorTests
{
    [Fact]
    public void NextKeepsEveryReadingWithinItsRealisticBounds()
    {
        var generator = new MetricsGenerator(new Random(Seed: 12345));

        for (var i = 0; i < 500; i++)
        {
            var snapshot = generator.Next();

            Assert.InRange(snapshot.CpuPercent, 5, 98);
            Assert.InRange(snapshot.ActiveUsers, 10, 500);
            Assert.InRange(snapshot.ErrorRatePercent, 0, 12);
        }
    }

    [Fact]
    public void NextIsDeterministicForAGivenSeed()
    {
        var first = new MetricsGenerator(new Random(Seed: 42)).Next();
        var second = new MetricsGenerator(new Random(Seed: 42)).Next();

        Assert.Equal(first.CpuPercent, second.CpuPercent);
        Assert.Equal(first.ActiveUsers, second.ActiveUsers);
        Assert.Equal(first.ErrorRatePercent, second.ErrorRatePercent);
    }

    [Fact]
    public void ConsecutiveReadingsEvolveGradually()
    {
        var generator = new MetricsGenerator(new Random(Seed: 7));

        var previous = generator.Next();
        for (var i = 0; i < 50; i++)
        {
            var current = generator.Next();

            // A single tick should never swing CPU by more than its configured max step.
            Assert.InRange(Math.Abs(current.CpuPercent - previous.CpuPercent), 0, 6.01);
            previous = current;
        }
    }
}
