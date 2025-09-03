using Microsoft.Extensions.FileProviders;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentAPlace.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});

// DbContext
builder.Services.AddDbContext<RentAPlaceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT configuration
var jwtCfg = builder.Configuration.GetSection("Jwt");
var keyBytes = Encoding.UTF8.GetBytes(jwtCfg["Key"] ?? throw new InvalidOperationException("JWT key missing"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtCfg["Issuer"],
        ValidAudience = jwtCfg["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Swagger (dev)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve Angular static files from "dist/RentAPlace.Web" when deployed
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

// session must be before controllers if you use HttpContext.Session in controllers
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SPA fallback: if no endpoint matched and not /api and not a file, return index.html
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

// For completeness
app.MapGet("/", () => "Backend running...");

app.Run();
