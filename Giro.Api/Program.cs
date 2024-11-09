using Giro.Api.Data;
using Giro.Api.Interfaces;
using Giro.Api.Middleware;
using Giro.Api.Services;
using Microsoft.EntityFrameworkCore;
//using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddSingleton<ICacheService, CacheService>();

//var redisConnectionString = builder.Configuration["RedisConnection"];
//builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHub<SignalRService>("/ws");

app.UseCors(builder =>
{
    builder
    //.AllowAnyOrigin()
    .WithOrigins("http://localhost:3000", "http://localhost:3001", "https://giro-alpha.vercel.app")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
});


string[] unprotectedRoutes = new string[] { "/api/auth", "/api/places", "/ws" };

app.UseWhen(context => {
    bool useMiddleware = true;
    foreach (var route in unprotectedRoutes)
    {
        if (context.Request.Path.StartsWithSegments(route))
        {
            useMiddleware = false;
            break;
        }
    }
    return useMiddleware;
}, builder =>
{
    builder.UseMiddleware<AuthMiddleware>();
});

app.MapControllers();

app.Run();


