
using ClientApp.Services;
using ClientApp.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Quartz;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);


// Initialize Serilog directly from the configuration
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithClientIp() // Add client IP enrichment
);



builder.Logging.ClearProviders();
//builder.Logging.AddSerilog(logger);





// Read Serilog settings from appsettings.json
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();  // This is important!
builder.Services.AddScoped<IJsonFileService, JsonFileService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddOptions<FileStorage>()
    .BindConfiguration(nameof(FileStorage));


// Add Quartz services
builder.Services.AddQuartz(q =>
{
    // Create a "DgiiFileDownloadJob" with the identity of "DgiiJob"
    var jobKey = new JobKey("DgiiJob");

// Register the job
q.AddJob<DgiiFileDownloadJob>(opts => opts.WithIdentity(jobKey));

// Create a trigger that runs daily at midnight
q.AddTrigger(opts => opts
    .ForJob(jobKey)
    .WithIdentity("DgiiJob-trigger")
    .WithCronSchedule(CronScheduleBuilder.DailyAtHourAndMinute(
        int.Parse(builder.Configuration["DgiiSettings:ScheduleHour"] ?? "0"),
        int.Parse(builder.Configuration["DgiiSettings:ScheduleMinute"] ?? "0")
    ))
);
});

// Add the Quartz.NET hosted service
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);



builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", 
        new OpenApiInfo 
        {
            Title = "RNC ", 
            Version = "v1",
            Description = "API para Consulta RNC. Descargado diariamente desde  la DGII:https://dgii.gov.do/app/WebApps/Consultas/RNC/DGII_RNC.zip ",


            Contact = new OpenApiContact
         {
             Email = "Oscar.portorreal@OscarSoft.Net",
             Name = "OscarSoft.net",
             Url = new Uri("https://www.OscarSoft.Net")

         },
            License = new OpenApiLicense
            {
                Name = "www.OscarSoft.net",
                Url = new Uri("https://www.OscarSoft.Net/licenses/")
            }



        });

      

});




// Add API documentation
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {

        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OscarSoft - Consulta RNC - DGII Ver 1");
      //  c.EnablePersistAuthorization();// This enables token persistence

    });
}
//app.UseOutputCache();
app.UseSerilogRequestLogging(); // Enable request logging

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
