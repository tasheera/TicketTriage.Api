using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using TicketTriage.Api;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");


//api call
builder.Services.AddHttpClient<GroqService>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
    var apiKey = builder.Configuration["GROQ_API_KEY"];
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    client.Timeout = TimeSpan.FromSeconds(10);
});


//CORS policy
var allowedOrigins = (builder.Configuration["ALLOWED_ORIGINS"] ?? "http://localhost:3000")
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.UseCors("FrontendPolicy");

app.MapControllers();

app.MapGet("/health", () => "OK");

app.MapGet("/test-groq", async (GroqService groq) =>
{
    var result = await groq.ClassifyTicketAsync(
        "Cannot log in",
        "I've tried resetting my password three times and I still can't get in. Deadline tomorrow."
    );
    return Results.Ok(result);
});

app.Run();
