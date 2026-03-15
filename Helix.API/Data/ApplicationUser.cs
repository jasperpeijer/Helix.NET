using Microsoft.AspNetCore.Identity;

namespace Helix.API.Data;

public class ApplicationUser : IdentityUser
{
    public ICollection<SmithWatermanAlignmentJob> Jobs { get; set; } = new List<SmithWatermanAlignmentJob>();

    public string SubscriptionTier { get; set; } = "Free";
}