using Xunit;
using Helix.CoreEngine;

namespace Helix.CoreEngine.Tests;

public class GenomeSequenceTests
{
    [Theory]
    // Perfect sequences
    [InlineData("GGCC", 1.0)] // 4/4 = 1.0
    [InlineData("AATT", 0.0)] // 0/4 = 0.0
    [InlineData("GCAT", 0.5)] // 2/4 = 0.5
    // Dirty sequences (edge cases)
    [InlineData("GCNX", 1.0)] // Valid: G,C. GC=2. 2/2 = 1.0
    [InlineData("GCATNN", 0.5)] // Valid: G,C,A,T. GC=2. 2/4 = 0.5
    [InlineData("NNNN", 0.0)] // Valid: None. Should not divide by 0!
    public void CalculateGcContent_ReturnsCorrectRatio(string rawSequence, double expectedRatio)
    {
        // Arrange
        var span = rawSequence.AsSpan();
        var sequence = new GenomicSequence(span);
        
        // Act
        double actualRatio = sequence.CalculateGcContent();
        
        // Assert
        Assert.Equal(expectedRatio, actualRatio, 5);
    }
}