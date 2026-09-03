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


//b-crypt password - initial agent
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (!db.Agents.Any())
    {
        var seedEmail = builder.Configuration["SEED_AGENT_EMAIL"];
        var seedPassword = builder.Configuration["SEED_AGENT_PASSWORD"];
        var seedName = builder.Configuration["SEED_AGENT_NAME"] ?? "Admin";

        if(!string.IsNullOrEmpty(seedEmail) && !string.IsNullOrEmpty(seedPassword))
        {
            var agent = new Agent
            {
                Name = seedEmail,
                Email = seedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(seedPassword),
                Role = AgentRole.Admin
            };

            db.Agents.Add(agent);
            db.SaveChanges();
        }
    }
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
