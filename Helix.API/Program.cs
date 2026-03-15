using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Helix.API;
using Helix.API.Data;
using Helix.CoreEngine;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// REGISTER CUSTOM SERVICES
builder.Services.AddScoped<Helix.API.Services.JobLimitService>();

// REGISTER SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token. Example: \"Bearer eyJhb...\""
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// REGISTER DATABASE STUFF
builder.Services.AddDbContext<HelixDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// REGISTER CORS POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:7009")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// REGISTER AUTHORIZATION & AUTHENTICATION
builder.Services.AddAuthorization();

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<HelixDbContext>();

// APP CONFIG
var app = builder.Build();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<ApplicationUser>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ENDPOINTS
app.MapPost("/api/v1/align/jobs", async ([FromBody] SmithWatermanAlignmentJobRequest jobRequest, HelixDbContext db,
        ClaimsPrincipal user, Helix.API.Services.JobLimitService jobLimitService) => 
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }
    
    var limitCheck = await jobLimitService.CheckLimitAsync(userId);

    if (!limitCheck.CanSubmit)
    {
        return Results.Problem(detail: limitCheck.Message, statusCode: StatusCodes.Status429TooManyRequests);
    }
    
    var job = new SmithWatermanAlignmentJob
    {
        Id = Guid.NewGuid(),
        SequenceA = jobRequest.SequenceA,
        SequenceB = jobRequest.SequenceB,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow,
        UserId = userId
    };
    
    db.SmithWatermanAlignmentJobs.Add(job);
    await db.SaveChangesAsync();
    
    // RABBITMQ
    var factory = new ConnectionFactory { HostName = "127.0.0.1" };
    
    await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync(
        queue: "smith_waterman_alignment_jobs",
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );
    
    var messagePayload = new { JobId = job.Id };
    var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messagePayload));

    var properties = new BasicProperties
    {
        Persistent = true,
    };
    
    await channel.BasicPublishAsync(
        exchange: string.Empty,
        routingKey: "alignment_jobs",
        body: body
    );
    
    return Results.Accepted($"/api/v1/align/jobs/{job.Id}", new { Id = job.Id, Status = job.Status });
})
.WithName("SubmitAlignmentJob")
.WithOpenApi()
.RequireAuthorization();

app.MapGet("/api/v1/align/jobs", async (HelixDbContext db, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    var jobs = await db.SmithWatermanAlignmentJobs
        // 2. Filter the database rows FIRST using the full model
        .Where(j => j.UserId == userId)
        // 3. Sort the filtered rows
        .OrderByDescending(job => job.CreatedAt)
        // 4. FINALLY, project the exact columns you want to send to the client
        .Select(job => new
        {
            job.Id,
            job.Status,
            job.CreatedAt,
            job.CompletedAt,
            job.FinalScore
        })
        .ToListAsync();
    
    return Results.Ok(jobs);
})
.WithName("GetAlignmentJobs")
.WithOpenApi()
.RequireAuthorization();

// GET: Check the status of a specific job
app.MapGet("/api/v1/align/jobs/{id:guid}", async (Guid id, HelixDbContext db, ClaimsPrincipal user) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    
    var job = await db.SmithWatermanAlignmentJobs.FindAsync(id);
    
    if (job == null)
    {
        return Results.NotFound(new { Error = "Job not found. Please double check your job ID" });
    }
    
    if (job.UserId != userId) return Results.Unauthorized();

    return Results.Ok(job);
})
.WithName("GetAlignmentJobStatus")
.WithOpenApi()
.RequireAuthorization();

app.Run();