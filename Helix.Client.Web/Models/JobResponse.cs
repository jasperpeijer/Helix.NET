namespace Helix.Client.Web.Models;

public class JobResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? FinalScore { get; set; }
    public string? AlignedSequenceA { get; set; }
    public string? AlignedSequenceB { get; set; }
    public double? IdentityPercentage { get; set; }
    public int? Matches { get; set; }
    public int? Mismatches { get; set; }
    public int? Gaps { get; set; }
}