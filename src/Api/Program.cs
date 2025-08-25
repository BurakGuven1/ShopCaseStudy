using Api.Auth;
using Api.Middlewares;
using Application;
using Infrastructure;
using Infrastructure.Health;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// MVC & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Sadece JWT yapıştıralım. (Bearer yazmaya gerek yok).",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { securityScheme, Array.Empty<string>() } });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, Api.Swagger.ConfigureSwaggerOptions>();

// Localization
builder.Services.AddLocalization(opt => opt.ResourcesPath = "Resources");
var supported = new[] { "tr-TR", "en-US" };
builder.Services.Configure<RequestLocalizationOptions>(opt =>
{
    opt.SetDefaultCulture("en-US");
    opt.AddSupportedCultures(supported);
    opt.AddSupportedUICultures(supported);
});

// API Versioning
builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new ApiVersion(1, 0);
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
    opts.ApiVersionReader = new UrlSegmentApiVersionReader();
});
builder.Services.AddVersionedApiExplorer(options =>
{
	options.GroupNameFormat = "'v'V";
	options.SubstituteApiVersionInUrl = true;
});


builder.Services.AddHttpsRedirection(o => o.HttpsPort = 443);

// MediatR & altyapı
var mediatRLicenseKey = builder.Configuration["MediatR:LicenseKey"]
    ?? Environment.GetEnvironmentVariable("MEDIATR_LICENSE_KEY")
    ?? throw new InvalidOperationException("MediatR License Key is required");
builder.Services.AddApplicationServices(mediatRLicenseKey);

builder.Services.AddInfrastructureServices(builder.Configuration);

// JWT doğrulama (middleware)
var jwt = builder.Configuration.GetSection("Jwt");
var issuer = jwt["Issuer"]!;
var audience = jwt["Audience"]!;
var key = jwt["Key"]!;
if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
    throw new InvalidOperationException("Jwt:Key missing or too short (min 32 chars)");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanWriteProducts", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole("Admin") ||
            ctx.User.HasClaim("perm", "product.write")));

    options.AddPolicy("OrderOwnerOrAdmin", policy =>
        policy.Requirements.Add(new OrderOwnerOrAdminRequirement()));
});

// Resource-based auth handler (SCOPED)
builder.Services.AddScoped<IAuthorizationHandler, OrderOwnerOrAdminHandler>();

var app = builder.Build();

// Pipeline
var locOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

//serilog req log
app.UseSerilogRequestLogging();


app.UseMiddleware<Api.Middlewares.ProblemDetailsMiddleware>();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CustomRateLimitingMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthJsonResponseWriter.Write
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = e => e.Tags.Contains("ready"),
    ResponseWriter = HealthJsonResponseWriter.Write
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthJsonResponseWriter.Write
});

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();

	var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
	app.UseSwaggerUI(options =>
	{
		foreach (var desc in provider.ApiVersionDescriptions)
		{
			options.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json",
									$"Shop API {desc.GroupName.ToUpperInvariant()}");
		}
		options.RoutePrefix = "swagger";
	});
}

app.Run();
public partial class Program { }
