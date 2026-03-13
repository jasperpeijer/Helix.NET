using Helix.API;
using Helix.API.Data;
using Helix.API.Workers;
using Helix.CoreEngine;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HelixDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register constant check for pending jobs
builder.Services.AddHostedService<AlignmentWorker>();

// Add CORS policy to allow the Blazor client to communicate with this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7009")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowBlazorClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// POST: Submit a new alignment job
app.MapPost("/api/v1/align/jobs", async ([FromBody] AlignmentRequest request, HelixDbContext db) => 
{
    // 1. Create the database record
    var job = new GenomicJob
    {
        Id = Guid.NewGuid(),
        SequenceA = request.SequenceA,
        SequenceB = request.SequenceB,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow,
    };
    
    // 2. Save it to the database
    db.GenomicJobs.Add(job);
    await db.SaveChangesAsync();
    
    // 3. Return the Tracking ID immediately
    // HTTP 202 Accepted means: "I received your data, and I will process it later."
    return Results.Accepted($"/api/v1/align/jobs/{job.Id}", new { Id = job.Id, Status = job.Status });
})
.WithName("SubmitAlignmentJob")
.WithOpenApi();

app.MapGet("/api/v1/align/jobs", async (HelixDbContext db) =>
{
    return await db.GenomicJobs
        .OrderByDescending(job => job.CreatedAt)
        .Select(job => new
        {
            job.Id,
            job.Status,
            job.CreatedAt,
            job.CompletedAt,
            job.FinalScore
        })
        .ToListAsync();
})
.WithName("GetAlignmentJobs")
.WithOpenApi();

// GET: Check the status of a specific job
app.MapGet("/api/v1/align/jobs/{id:guid}", async (Guid id, HelixDbContext db) =>
{
    var job = await db.GenomicJobs.FindAsync(id);

    if (job == null)
    {
        return Results.NotFound(new { Error = "Job not found. Please double check your job ID" });
    }

    return Results.Ok(job);
})
.WithName("GetAlignmentJobStatus")
.WithOpenApi();

app.Run();