using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore; // EF Core
using Hogwarts.Data; // Data sloj
using Hogwarts.Services; // NOVO: Services sloj za Inventar
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Konfiguracija EF Core da koristi PostgreSQL bazu
builder.Services.AddDbContext<HogwartsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    
    // Dodatne opcije za development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Register kontrolere
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Konfiguracija za JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Swagger (za razvojnu okolinu)
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Hogwarts HR API",
            Version = "v1",
            Description = "API za HR sistem kompanije Hogwarts"
        });

        // Konfiguracija za JWT autentifikaciju u Swagger-u
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] { }
            }
        });
    });
}

// NOVO: Inventar Service DI registracija
builder.Services.AddScoped<IInventarService, InventarService>();

// JWT autentifikacija
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "HogwartsSecretKey123456789AbcDefGhiJklMnoPqrStUvWxYz!@#$%^&*()";
    var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Hogwarts";
    var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Hogwarts";
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero // Smanji toleranciju na istekle tokene
    };
});

// Autorizacija sa policy-jima
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("HRAccess", policy => policy.RequireRole("SuperAdmin", "HRManager"));
    options.AddPolicy("ManagementAccess", policy => policy.RequireRole("SuperAdmin", "HRManager", "TeamLead"));
});

// CORS politika za frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactOrigin",
        builder => builder
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Završavamo konfiguraciju
var app = builder.Build();

// Auto-migracija u development-u
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<HogwartsContext>();
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hogwarts HR API v1");
        c.RoutePrefix = string.Empty; // Swagger na root URL-u
    });
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// CORS mora biti pre autentifikacije
app.UseCors("AllowReactOrigin");

// Autentifikacija i autorizacija
app.UseAuthentication();
app.UseAuthorization();

// Mapiranje kontrolera
app.MapControllers();

// Enable static file serving
app.UseStaticFiles(); 

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
}));

app.Run();