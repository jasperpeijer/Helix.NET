using System.Diagnostics;
using Helix.CoreEngine;

Console.WriteLine("Generating 10M length DNA sequence...");

var random = new Random();

char[] nucleotides = {'A', 'C', 'G', 'T'};
char[] sequenceArray = new char[10_000_000];

for (int i = 0; i < sequenceArray.Length; i++)
{
    sequenceArray[i] = nucleotides[random.Next(4)];
}

Console.WriteLine("Analyzing sequence with domain model....");

var stopwatch = Stopwatch.StartNew();

GenomicSequence genome = new GenomicSequence(sequenceArray);

double gcContent = genome.CalculateGcContent(true);

stopwatch.Stop();

Console.WriteLine("--- Results ---");
Console.WriteLine($"Total Base Pairs: {genome.Length:N0}");
Console.WriteLine($"GC-Content: {gcContent:F2}%");
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
