using Locus.Api.Data;
using Locus.Api.Data.Seeders;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Define the CORS policy name
var allowFrontend = "_allowFrontend";

// Add services
builder.Services.AddControllers();

// 2. Add CORS service
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: allowFrontend,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173") // Your React/Vite URL
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Locus API", Version = "v1" });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Locus API v1");
        c.RoutePrefix = "swagger";
    });
}

// 3. Enable CORS in the pipeline
// It must be placed after UseRouting (if used) and before MapControllers
app.UseCors(allowFrontend);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        DatabaseSeeder.SeedRooms(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
