using DistributedSynchronizedBackgroundJobs.BackgroundJobs;
using DistributedSynchronizedBackgroundJobs.Cache;
using DistributedSynchronizedBackgroundJobs.NumericRandomGenerator;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<INumericRandomGenerator, UniqueAcrossMultipleInstancesRunAtSameTimeNumericRandomGenerator>();

// Redis Master/Slave Pattern
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
    options.InstanceName = builder.Configuration["Redis:ChannelPrefix"];
});
var redisMaster = builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ReplicasConnectionString"];
    options.InstanceName = builder.Configuration["Redis:ChannelPrefix"];
});
var redisSlave = builder.Services.BuildServiceProvider().GetRequiredService<IDistributedCache>();

builder.Services.AddSingleton<ICache>(_cache => new RedisCache(redisMaster, redisSlave));


builder.Services.AddHostedService<TimedHostedBackgroundService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
