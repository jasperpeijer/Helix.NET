using Helix.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Helix.API.Services;

public class JobLimitService
{
    private readonly HelixDbContext _db;

    public JobLimitService(HelixDbContext db)
    {
        _db = db;
    }

    public async Task<(bool CanSubmit, string Message)> CheckLimitAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null) return (false, "User not found.");

        int monthlyLimit = user.SubscriptionTier == "Premium" ? 200 : 40;
        
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        
        var thisMonthJobs = await _db.SmithWatermanAlignmentJobs
            .CountAsync(job => job.UserId == userId && job.CreatedAt >= startOfMonth && job.CreatedAt <= now);

        if (thisMonthJobs >= monthlyLimit)
        {
            return (false,
                "$\"Monthly limit exceeded. Your {user.SubscriptionTier} tier allows {monthlyLimit} jobs per month. You have used {thisMonthsJobs} in {now:MMMM}. Limits reset on the 1st of next month.\"");
        }
        
        return (true, "Limit check passed.");
    }
}