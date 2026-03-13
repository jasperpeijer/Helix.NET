namespace Helix.CoreEngine;

public readonly ref struct GenomicSequence(ReadOnlySpan<char> sequence)
{
    private readonly ReadOnlySpan<char> _sequence = sequence;

    public int Length => _sequence.Length;

    public double CalculateGcContent(bool percentage = false)
    {
        if (_sequence.IsEmpty) return 0.0;

        int gcCount = 0;
        int validBases = 0;

        foreach (char nucleotide in _sequence)
        {
            if (nucleotide == 'G' || nucleotide == 'C')
            {
                gcCount++;
                validBases++;
            }
            else if (nucleotide == 'A' || nucleotide == 'T')
            {
                validBases++;
            }
        }
        
        if (validBases == 0) return 0.0;
        
        return (double) gcCount / validBases * (percentage ? 100 : 1);
    }
    
    public char this[int index] => _sequence[index];
}