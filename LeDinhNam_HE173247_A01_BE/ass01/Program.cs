using ass01.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ass01.BusinessLogic.DTOs.NewsArticle;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("https://localhost:7004")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddSignalR();

// Add HttpContextAccessor for AuditLogService
builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddDbContext<FunewsManagementContext>(
options => options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
);

// Register DAOs
builder.Services.AddScoped<ass01.DataAccess.DAOs.IAccountDAO, ass01.DataAccess.DAOs.AccountDAO>();
builder.Services.AddScoped<ass01.DataAccess.DAOs.ICategoryDAO, ass01.DataAccess.DAOs.CategoryDAO>();
builder.Services.AddScoped<ass01.DataAccess.DAOs.INewsArticleDAO, ass01.DataAccess.DAOs.NewsArticleDAO>();
builder.Services.AddScoped<ass01.DataAccess.DAOs.ITagDAO, ass01.DataAccess.DAOs.TagDAO>();
builder.Services.AddScoped<ass01.DataAccess.DAOs.IRefreshTokenDAO, ass01.DataAccess.DAOs.RefreshTokenDAO>();
builder.Services.AddScoped<ass01.DataAccess.DAOs.IAuditLogDAO, ass01.DataAccess.DAOs.AuditLogDAO>();

// Register Repositories
builder.Services.AddScoped<ass01.DataAccess.Repositories.IAccountRepository, ass01.DataAccess.Repositories.AccountRepository>();
builder.Services.AddScoped<ass01.DataAccess.Repositories.ICategoryRepository, ass01.DataAccess.Repositories.CategoryRepository>();
builder.Services.AddScoped<ass01.DataAccess.Repositories.INewsArticleRepository, ass01.DataAccess.Repositories.NewsArticleRepository>();
builder.Services.AddScoped<ass01.DataAccess.Repositories.ITagRepository, ass01.DataAccess.Repositories.TagRepository>();
builder.Services.AddScoped<ass01.DataAccess.Repositories.IRefreshTokenRepository, ass01.DataAccess.Repositories.RefreshTokenRepository>();
builder.Services.AddScoped<ass01.DataAccess.Repositories.IAuditLogRepository, ass01.DataAccess.Repositories.AuditLogRepository>();

// Register Services
builder.Services.AddScoped<ass01.BusinessLogic.Services.IAuthService, ass01.BusinessLogic.Services.AuthService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.IAccountService, ass01.BusinessLogic.Services.AccountService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.ICategoryService, ass01.BusinessLogic.Services.CategoryService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.ITagService, ass01.BusinessLogic.Services.TagService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.IAuditLogService, ass01.BusinessLogic.Services.AuditLogService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.INewsArticleService, ass01.BusinessLogic.Services.NewsArticleService>();
builder.Services.AddScoped<ass01.BusinessLogic.Services.IReportService, ass01.BusinessLogic.Services.ReportService>();

// Configure OData EDM Model
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<NewsArticleDto>("NewsArticle");
    return builder.GetEdmModel();
}

builder.Services.AddControllers()
    .AddOData(options => options
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
        .AddRouteComponents("odata", GetEdmModel())
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ass01.Presentation.Hubs.NotificationHub>("/hubs/notifications");

// Seed database (idempotent: only inserts if data does not exist)
await ass01.DataAccess.DbSeeder.SeedAsync(app.Services);

app.Run();

