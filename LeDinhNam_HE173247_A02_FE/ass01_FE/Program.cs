using ass01_FE.DataAccess.Services;
using ass01_FE.BusinessLogic.Services;
using ass01_FE.Infrastructure;
using ass01_FE.Infrastructure.Clients;
using ass01_FE.Infrastructure.Handlers;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/Presentation/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Presentation/Views/Shared/{0}.cshtml");
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<TokenRefreshCoordinator>();
builder.Services.AddTransient<ApiLoggingHandler>();
builder.Services.AddTransient<AuthenticatedHttpClientHandler>();
builder.Services.AddScoped<ass01_FE.Infrastructure.Services.IOfflineCacheService, ass01_FE.Infrastructure.Services.OfflineCacheService>();
builder.Services.AddScoped<ass01_FE.Infrastructure.Services.OfflineNewsService>();
builder.Services.AddScoped<ass01_FE.Infrastructure.Services.OfflineCategoryService>();
builder.Services.AddScoped<ass01_FE.Infrastructure.Services.OfflineTagService>();
builder.Services.AddScoped<ass01_FE.Infrastructure.Services.OfflineDashboardService>();
builder.Services.AddSingleton<ass01_FE.Infrastructure.Services.WorkerTokenService>();
builder.Services.AddHostedService<ass01_FE.Infrastructure.Workers.CacheRefreshWorker>();

// Polly Policy (Idempotent only: GET/HEAD)
static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

// Ensure ApiSettings exist
var coreApiUrl = builder.Configuration["ApiSettings:CoreApi"] ?? "https://localhost:7001/";
var analyticsApiUrl = builder.Configuration["ApiSettings:AnalyticsApi"] ?? "https://localhost:7002/";
var aiApiUrl = builder.Configuration["ApiSettings:AiApi"] ?? "https://localhost:7003/";

// 1. AuthRefreshClient (NO AuthHandler, NO generic Polly retry)
builder.Services.AddHttpClient<AuthRefreshClient>(client =>
{
    client.BaseAddress = new Uri(coreApiUrl);
})
.AddHttpMessageHandler<ApiLoggingHandler>();

// 2. ApiServices Migration (BaseAddress: Core, AuthHandler, Polly)
builder.Services.AddHttpClient<AuthApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<NewsApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<AuditLogApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<AccountApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<ReportApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<CategoryApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<TagApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<ProfileApiService>(client => client.BaseAddress = new Uri(coreApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

// 3. New Clients for Analytics & AI
builder.Services.AddHttpClient<AnalyticsApiClient>(client => client.BaseAddress = new Uri(analyticsApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddHttpClient<AiApiClient>(client => client.BaseAddress = new Uri(aiApiUrl))
    .AddHttpMessageHandler<ApiLoggingHandler>()
    .AddHttpMessageHandler<AuthenticatedHttpClientHandler>()
    .AddPolicyHandler(request => request.Method == HttpMethod.Get || request.Method == HttpMethod.Head ? GetRetryPolicy() : Policy.NoOpAsync<HttpResponseMessage>());

builder.Services.AddScoped<NewsViewService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
