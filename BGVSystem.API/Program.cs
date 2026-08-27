using BGVSystem.API.Middleware;
using BGVSystem.Application.Interfaces;
using BGVSystem.Application.Services;
using BGVSystem.Infrastructure.BackgroundServices;
using BGVSystem.Infrastructure.Services;
using BGVSystem.Infrastructure.Settings;
using BGVSystem.Persistence.Context;
using BGVSystem.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");

var builder = WebApplication.CreateBuilder(args);

// Disable file watchers on Linux containers to avoid inotify handle limits
builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
{
    foreach (var source in config.Sources.OfType<Microsoft.Extensions.Configuration.FileConfigurationSource>())
    {
        source.ReloadOnChange = false;
    }
});

// Database Configuration

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(
        "EmailSettings"));
builder.Services.Configure<SupabaseSettings>(
    builder.Configuration.GetSection(
        "SupabaseSettings"));

builder.Services.AddHttpClient<IFileStorageService, SupabaseStorageService>();

builder.Services
    .AddHostedService<
        NotificationWorker>();
// Dependency Injection

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<ICandidateService, CandidateService>();

builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();

builder.Services.AddScoped<IVerificationRepository, VerificationRepository>();
builder.Services.AddScoped<IVerificationService, VerificationService>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailTemplateService,EmailTemplateService>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddScoped<ICandidatePortalService, CandidatePortalService>();

builder.Services.AddScoped<IReviewerService, ReviewerService>();
builder.Services.AddScoped<
    IAssignmentRepository,
    AssignmentRepository>();
builder.Services.AddScoped<
    IReportService,
    ReportService>();
builder.Services.AddScoped<
    IAssignmentService,
    AssignmentService>();
builder.Services
    .AddScoped<IReviewerDocumentService,
               ReviewerDocumentService>();

builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IThirdPartyVerificationService, ThirdPartyVerificationService>();


builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<
    IAdminDashboardService,
    AdminDashboardService>();
// JWT Authentication

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!))
            };
    });

// Controllers

builder.Services.AddControllers();

// Swagger Configuration

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ReactPolicy",
        policy =>
        {
            policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
        });
});


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BGV API",
        Version = "v1"
    });

    // JWT Auth Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // JWT Requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



var app = builder.Build();

// Automatically apply EF Core database migrations on startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();

        dbContext.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT 1 FROM Roles WHERE Id = 1)
            BEGIN
                SET IDENTITY_INSERT Roles ON;
                INSERT INTO Roles (Id, Name) VALUES (1, 'Admin');
                INSERT INTO Roles (Id, Name) VALUES (2, 'Reviewer');
                INSERT INTO Roles (Id, Name) VALUES (3, 'Candidate');
                SET IDENTITY_INSERT Roles OFF;
            END
        ");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations or seeding roles on startup.");
    }
}

// Swagger Middleware

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "BGV System API V1");

        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Global Exception Middleware

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("ReactPolicy");
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();