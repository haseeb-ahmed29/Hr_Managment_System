using Hangfire;
using HrManagmentSystem_API.Extension_Method;
using HrManagmentSystem_API.Middleware;
using HrMangmentSystem_API.Extension_Method;
using HrMangmentSystem_Application.Config;
using HrMangmentSystem_Application.Extension_Method;
using HrMangmentSystem_Infrastructure.Extension_Method;
using Serilog;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationService();
builder.Services.AddAutoMapperProfiles();
builder.Services.AddConfigureDatabases(builder.Configuration);
builder.Services.AddLocaizationResource(builder.Configuration) ;
builder.Services.AddLeaveAccrualQuartz(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration); 
builder.Services.AddHangfireWithJobs(builder.Configuration);
builder.Services.AddCustomHealthChecks(builder.Configuration); 
builder.Services.AddOpenAi(builder.Configuration);
builder.AddSerilogLogging();
builder.Services.AddAppRateLimiting();
builder.Services.Configure<LeaveAccrualOptions>(builder.Configuration.GetSection("LeaveAccrual"));
builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
builder.Services.Configure<LeaveBalanceOptions>(builder.Configuration.GetSection("LeaveBalance"));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

builder.Services.AddControllers()
              .AddJsonOptions(options =>
              {
                  options.JsonSerializerOptions.Converters.Add(
                      new JsonStringEnumConverter());
              });

builder.Services.AddLocalization(options => options.ResourcesPath = "");


builder.Services.AddAuthorization();




// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
if (app.Configuration.GetValue("Hangfire:Enabled", false))
    app.UseHangfireDashboard();
app.UseSerilogRequestLogging();

//await DataSeeding.SeedAsync(app.Services);


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAuthentication();

app.UseAddLocalization();

app.UseMiddleware<CurrentTenantMiddleware>(); //after Authentication : because httpContext.User fill from JWT 
app.UseHangfireRecurringJobs();
app.UseHttpsRedirection();
app.MapCustomHealthChecks(); 
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
