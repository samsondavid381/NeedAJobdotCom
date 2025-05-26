using Microsoft.EntityFrameworkCore;
using NeedAJobdotCom.Infrastructure.Data;
using NeedAJobdotCom.Core.Interfaces;
using NeedAJobdotCom.Infrastructure.Repositories;
using NeedAJobdotCom.Core.Services;
using NeedAJobdotCom.Infrastructure.ExternalServices;
using NeedAJobdotCom.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Railway port configuration
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<JobBoardContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HTTP Client for external APIs
builder.Services.AddHttpClient<AdzunaService>();

// Add Repository and Services
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IAdzunaService, AdzunaService>();
builder.Services.AddScoped<IJobAggregator, JobAggregator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();