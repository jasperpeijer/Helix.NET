using System;
using Xunit;
using Helix.CoreEngine;

namespace Helix.CoreEngine.Tests;

public class LocalAlignerTests
{
    [Fact]
    public void ComputeAlignment_FindsOptimalLocalIsland()
    {
        // Arrange
        var seq1 = new GenomicSequence("ACAC".AsSpan());
        var seq2 = new GenomicSequence("CACT".AsSpan());
        
        // Matrix dimensions must be Length + 1 to account for the leading zeroes 
        // required by the dynamic programming base cases.
        var matrix = new ScoringMatrix(seq1.Length + 1, seq2.Length + 1);
        
        // Set up the biological scoring parameters
        var aligner = new LocalAligner(matchScore: 3, mismatchPenalty: -3, gapPenalty: -2);
        
        // Act
        var result = aligner.ComputeAlignment(seq1, seq2, ref matrix);
        
        // Assert
        // The optimal island is "CAC" (3 matches * 3 points = 9)
        Assert.Equal(9, result.Score);
        
        // The optimal island in both sequences is "CAC", with no gaps needed.
        Assert.Equal("CAC", result.AlignedRow);
        Assert.Equal("CAC", result.AlignedCol);
    }

    [Fact]
    public void ComputeAlignment_InsertsGapToMaximizeScore()
    {
        // Arrange
        var seq1 = new GenomicSequence("GATA".AsSpan());
        var seq2 = new GenomicSequence("GATCA".AsSpan());
        var matrix = new ScoringMatrix(seq1.Length + 1, seq2.Length + 1);
        var aligner = new LocalAligner(matchScore: 3, mismatchPenalty: -3, gapPenalty: -2);
        
        // Act
        var result = aligner.ComputeAlignment(seq1, seq2, ref matrix);
        
        // Assert
        // 4 matches (12 points) + 1 gap penalty (-2 points) = 10 points.
        Assert.Equal(10, result.Score);
        
        // Sequence 1 must contain a gap (-) to skip the 'C' in Sequence 2
        Assert.Equal("GAT-A", result.AlignedRow);
        Assert.Equal("GATCA", result.AlignedCol);
    }
}