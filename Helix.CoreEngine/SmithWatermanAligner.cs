namespace Helix.CoreEngine;

/// <summary>
/// Executes the Smith-Waterman local sequence alignment algorithm.
/// </summary>
public readonly struct SmithWatermanAligner
{
    private readonly int _match;
    private readonly int _mismatch;
    private readonly int _gap;

    /// <summary>
    /// Executes the Smith-Waterman local sequence alignment algorithm.
    /// </summary>
    public SmithWatermanAligner(int matchScore, int mismatchPenalty, int gapPenalty)
    {
        _match = matchScore;
        _mismatch = mismatchPenalty > 0 ? -mismatchPenalty : mismatchPenalty;
        _gap = gapPenalty > 0 ? -gapPenalty : gapPenalty;
    }

    /// <summary>
    /// Computes the alignment matrix and returns the highest local score.
    /// </summary>
    public AlignmentResult ComputeAlignment(GenomicSequence rowSeq, GenomicSequence colSeq, ref ScoringMatrix matrix)
    {
        int maxGlobal = 0;
        int maxR = 0;
        int maxC = 0;

        // We start at 1 to skip the zero-row and zero-column base cases.
        // This inherently prevents the 1D wrap-around bug.
        for (int r = 1; r <= rowSeq.Length; r++)
        {
            for (int c = 1; c <= colSeq.Length; c++)
            {
                // 1. Determine if the current nucleotides match.
                // Note: We subtract 1 from r and c because our sequence strings 
                // are 0-indexed, but our matrix is 1-indexed (due to the zero-row).
                char rowChar =  rowSeq[r - 1];
                char colChar = colSeq[c - 1];
                
                int score = (rowChar == colChar) ? _match : _mismatch;
                
                // 2. Calculate the three possible paths
                int diagonalPath = matrix[r - 1, c - 1] + score;
                int upPath = matrix[r - 1, c] + _gap;
                int leftPath = matrix[r, c - 1] + _gap;
                
                // 3. Find the maximum of the three paths
                int maxLocal = Math.Max(diagonalPath, Math.Max(upPath, leftPath));

                // 4. Smith-Waterman Rule: If the score drops below 0, reset to 0.
                maxLocal = Math.Max(maxLocal, 0);
                
                // 5. Store the calculated score in our contiguous memory struct
                matrix[r, c] = maxLocal;
                
                // 6. Track the highest score seen anywhere in the entire grid
                if (maxLocal > maxGlobal)
                {
                    maxGlobal = maxLocal;
                    maxR = r;
                    maxC = c;
                }
            }
        }
        
        // Traceback
        int currR = maxR;
        int currC = maxC;
        
        // The maximum possible length of an alignment is the sum of both sequences.
        char[] alignedRow = new char[rowSeq.Length + colSeq.Length];
        char[] alignedCol = new char[colSeq.Length + rowSeq.Length];

        // We fill the arrays from right to left (backwards)
        int writeIndex = alignedRow.Length - 1;
        
        int matches = 0, mismatches = 0, gaps = 0;
        
        // Walk backwards until we hit a 0
        while (matrix[currR, currC] != 0)
        {
            int currentScore = matrix[currR, currC];
            char rChar = rowSeq[currR - 1];
            char cChar =  colSeq[currC - 1];
            int matchScore = (rChar == cChar) ? _match : _mismatch;
            
            // Where did the score come from?
            if (currentScore == matrix[currR - 1, currC - 1] + matchScore)
            {
                // This is for the identity percentage
                if (rChar == cChar) matches++;
                else mismatches++;
                
                // Came from diagonal: Match or Mismatch
                alignedRow[writeIndex] = rChar;
                alignedCol[writeIndex] = cChar;
                currR--;
                currC--;
            }
            else if (currentScore == matrix[currR - 1, currC] + _gap)
            {
                // This is for the identity percentage
                gaps++;
                
                // Came from Up: Gap in the Column sequence
                alignedRow[writeIndex] = rChar;
                alignedCol[writeIndex] = '-';
                currR--;
            }
            else
            {
                // This is for the identity percentage
                gaps++;
                
                // Came from Left: Gap in the Row sequence
                alignedRow[writeIndex] = '-';
                alignedCol[writeIndex] = cChar;
                currC--;
            }
            
            writeIndex--;
        }
        
        // --- 3. EXTRACT THE FINAL STRINGS ---
        // The writeIndex is pointing to one space BEFORE our alignment started.
        // We calculate the length, and use AsSpan to extract exactly what we need without garbage data.
        int length = alignedRow.Length - 1 - writeIndex;
        string finalRowSeq = new string(alignedRow.AsSpan(writeIndex + 1, length));
        string finalColSeq = new string(alignedCol.AsSpan(writeIndex + 1, length));

        double identityPercentage = length == 0 ? 0 : Math.Round((double)matches / length * 100, 2);
        
        return new AlignmentResult(maxGlobal, finalRowSeq, finalColSeq, 
            identityPercentage, matches, mismatches, gaps);
    }
}


/// <summary>
/// Represents the final computed local alignment between two sequences.
/// </summary>
public readonly record struct AlignmentResult(int Score, string AlignedRow, string AlignedCol, 
    double IdentityPercentage, int Matches, int Mismatches, int Gaps);