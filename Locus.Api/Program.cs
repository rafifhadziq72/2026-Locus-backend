using System.Text;
using Locus.Api.Data;
using Locus.Api.Data.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Required
using Microsoft.AspNetCore.Identity; // Required
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens; // Required

var builder = WebApplication.CreateBuilder(args);
var allowFrontend = "_allowFrontend";

builder.Services.AddControllers();

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

// --- START IDENTITY & JWT CONFIGURATION ---
builder
    .Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Development-friendly settings (Simpler Passwords)
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 4; // Min 4 characters instead of 8
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;

        // Ensure unique emails
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "A_Very_Long_Secret_Key_For_Testing_Only");

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

// --- END IDENTITY & JWT CONFIGURATION ---

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

// Order is critical here
app.UseAuthentication();
app.UseAuthorization();

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
        DatabaseSeeder.SeedRooms(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
