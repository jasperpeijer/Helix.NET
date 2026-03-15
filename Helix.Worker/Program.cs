using Helix.API.Data;
using Helix.API.Workers;
using Helix.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<HelixDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<AlignmentEngineWorker>();

var host = builder.Build();
host.Run();