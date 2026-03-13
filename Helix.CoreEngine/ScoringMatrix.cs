namespace Helix.CoreEngine;

/// <summary>
/// A high-performance, contiguous memory matrix for dynamic programming.
/// </summary>
public struct ScoringMatrix(int rows, int cols)
{
    private readonly int[] _data = new int[rows * cols];
    
    public int Rows { get; } = rows;
    public int Cols { get; } = cols;

    // The Indexer: This allows the matrix to be used like matrix[r, c] 
    // while executing the high-speed 1D math under the hood.
    public int this[int row, int col]
    {
        get => _data[row * cols + col];
        set => _data[row * cols + col] = value;
    }
    
    /// <summary>
    /// Exposes the raw memory for extreme optimization (e.g., SIMD hardware intrinsics).
    /// </summary>
    public Span<int> AsSpan() => _data.AsSpan();
}