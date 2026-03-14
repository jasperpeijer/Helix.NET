using System;
using System.Threading;
using System.Threading.Tasks;
using Helix.API.Data;
using Helix.CoreEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Helix.API.Workers;

public class AlignmentWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AlignmentWorker> _logger;
    
    public AlignmentWorker(IServiceProvider serviceProvider, ILogger<AlignmentWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Helix.NET Alignment Worker thread has started.");

        // This infinite loop runs as long as the server is turned on
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Create a safe, temporary scope for our Scoped database
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HelixDbContext>();
            
            // 2. Find the oldest "Pending" job in PostgreSQL
            var job = await db.GenomicJobs
                .FirstOrDefaultAsync(j => j.Status == "Pending", stoppingToken);

            if (job != null)
            {
                try
                {
                    // 3. Mark it as Processing so no other worker grabs it
                    _logger.LogInformation($"[{DateTime.UtcNow:HH:mm:ss}] Processing Job: {job.Id}");
                    job.Status = "Processing";
                    await db.SaveChangesAsync(stoppingToken);
                    
                    // 4. FIRE UP THE ENGINE! 
                    // (Converting the database strings back into our high-performance spans)
                    var result = RunAlignmentEngine(job.SequenceA, job.SequenceB);
                    
                    // 5. Save the scientific results back to the database
                    job.FinalScore = result.Score;
                    job.AlignedSequenceA = result.AlignedRow;
                    job.AlignedSequenceB = result.AlignedCol;
                    
                    // Metrics
                    job.IdentityPercentage = result.IdentityPercentage;
                    job.Matches = result.Matches;
                    job.Mismatches = result.Mismatches;
                    job.Gaps = result.Gaps;
                    
                    job.Status = "Completed";
                    job.CompletedAt = DateTime.UtcNow;
                    
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"[{DateTime.UtcNow:HH:mm:ss}] Job {job.Id} completed. Max Score: {result.Score}");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to process Job: {job.Id}");
                    job.Status = "Failed";
                    await db.SaveChangesAsync(stoppingToken);
                }
            }
            else
            {
                // 6. If the database is empty, put the thread to sleep for 3 seconds 
                // so we don't accidentally max out the CPU by checking an empty table millions of times a second.
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
    
    // Because there is no "async" keyword here, the compiler allows "ref" 
    // because it guarantees this stack frame won't be paused or torn down.
    private AlignmentResult RunAlignmentEngine(string sequenceA, string sequenceB)
    {
        var seqA = new GenomicSequence(sequenceA.AsSpan());
        var seqB = new GenomicSequence(sequenceB.AsSpan());
        
        var matrix = new ScoringMatrix(seqA.Length + 1, seqB.Length + 1);
        var aligner = new LocalAligner(3, -3, -2);
        
        return aligner.ComputeAlignment(seqA, seqB, ref matrix);
    }
}