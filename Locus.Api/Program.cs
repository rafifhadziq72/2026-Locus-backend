using System.Text;
using Locus.Api.Data;
using Locus.Api.Data.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);
var allowFrontend = "_allowFrontend";

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: allowFrontend,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
        }
    );
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Identity configuration remains for DB consistency
builder
    .Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Locus API", Version = "v1" });
});

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

app.UseCors(allowFrontend);

// AUTHENTICATION BYPASS: Commented out to allow public access to Web views
// app.UseAuthentication();
// app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // FIXED: Using your actual method name "SeedRooms"
        DatabaseSeeder.SeedRooms(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
