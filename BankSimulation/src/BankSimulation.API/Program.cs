using System.Diagnostics;
using System.Threading.RateLimiting;
using BankSimulation.API.Services;
using BankSimulation.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES ====================

builder.Services.AddSingleton<DapperContext>();      // Dapper Context (veritabani baglantisi)
builder.Services.AddScoped<DataSeeder>();            // DataSeeder (test verisi uretici)
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Enum'lari string olarak serileştir (frontend ile uyumlu)
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Validation davranisi (daha temiz 400 cevabi)
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        });
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bank Simulation API",
        Version = "v1.0",
        Description = "38 Tablolu Kapsamli Banka Simulasyonu - VTYS Ders Projesi",
        Contact = new OpenApiContact
        {
            Name = "Batuhan",
            Email = "batuhan@example.com"
        }
    });

    options.TagActionsBy(api =>
    {
        if (api.GroupName != null) return new[] { api.GroupName };
        if (api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller))
            return new[] { controller };
        return new[] { "Other" };
    });

    options.OrderActionsBy(api => api.RelativePath);
});

// CORS (frontend icin)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Basit global rate limiting (100 req/dk, 20 kuyruk)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
        RateLimitPartition.GetFixedWindowLimiter("global", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 20,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// ==================== MIDDLEWARE ====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank Simulation API v1");
        options.DocumentTitle = "Bank Simulation API";
        options.DefaultModelsExpandDepth(-1); // Model semalarini gizle
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRateLimiter();

// Basit istek loglama
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RequestLog");
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    logger.LogInformation("{Method} {Path} => {Status} ({Elapsed} ms)",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        sw.ElapsedMilliseconds);
});

app.MapControllers();

// ==================== STARTUP LOG ====================
Console.WriteLine(new string('-', 68));
Console.WriteLine("'           BANK SIMULATION API - VTYS DERS PROJESI            '");
Console.WriteLine("'  ->  38 Tablo | 9 Modul | Dapper + Pure SQL                  '");
Console.WriteLine("'  ->  Swagger: http://localhost:5161/swagger                  '");
Console.WriteLine(new string('-', 68));

app.Run();
