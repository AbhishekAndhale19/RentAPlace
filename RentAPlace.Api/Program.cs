using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentAPlace.Domain.Models;
using RentAPlace.Api.Services;

using RentAPlace.Api;
{
    
}

var builder = WebApplication.CreateBuilder(args);

// ---------------- Services ----------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//  Swagger + JWT security definition
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RentAPlace API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT}\nExample: Bearer eyJhbGciOiJIUzI1NiIs..."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS for Angular dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//email

builder.Services.AddSingleton(new Email(
    smtpHost: "smtp.gmail.com",       // or your SMTP provider
    smtpPort: 587,
    smtpUser: "abhishekandhale16368@gmail.com",
    smtpPass: "iisx pwki ruoi pmai"
));


// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

// DbContext
builder.Services.AddDbContext<RentAPlaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b=> b.MigrationsAssembly("RentAPlace.Application")
    ));

// JWT configuration
var jwtCfg = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtCfg["Key"]
    ?? throw new InvalidOperationException("JWT key missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;   // set true in production behind HTTPS
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtCfg["Issuer"],
        ValidAudience = jwtCfg["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

await AdminPasswordFix.FixAsync(app.Services);

// ---------------- Middleware ----------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve Angular static files (for production build)
var angularDist = Path.Combine(Directory.GetCurrentDirectory(), "dist", "RentAPlace.Web");
if (Directory.Exists(angularDist))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(angularDist),
        RequestPath = ""
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(angularDist),
        RequestPath = ""
    });
}

app.UseRouting();
app.UseCors("AllowAngular");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// SPA fallback to index.html for client routes
app.Use(async (context, next) =>
{
    await next();

    var path = context.Request.Path.Value ?? "";
    var isApi = path.StartsWith("/api");
    var hasExt = Path.HasExtension(path);

    if (context.Response.StatusCode == 404 && !isApi && !hasExt && Directory.Exists(angularDist))
    {
        context.Response.StatusCode = 200;
        await context.Response.SendFileAsync(Path.Combine(angularDist, "index.html"));
    }
});

app.MapGet("/", () => "Backend running...");

app.Run();
