namespace Helix.API.Data;

/// <summary>
/// Represents a sequence alignment job submitted to the Helix.NET platform.
/// </summary>
public class GenomicJob
{
    public Guid Id { get; set; }

    public string SequenceA { get; set; } = string.Empty;
    public string SequenceB { get; set; } = string.Empty;
    
    // The state of the job (e.g., "Pending", "Processing", "Completed", "Failed")
    public string Status { get; set; } = "Pending";
    
    // The results (nullable, because they don't exist until the job finishes)
    public int? FinalScore { get; set; }
    public string? AlignedSequenceA { get; set; }
    public string? AlignedSequenceB { get; set; }
    
    public double? IdentityPercentage { get; set; }
    public int? Matches { get; set; }
    public int? Mismatches { get; set; }
    public int? Gaps { get; set; }
    
    // Telemetry
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}