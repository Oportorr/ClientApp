
using ClientApp.Services;
using ClientApp.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);





// Add services to the container
builder.Services.AddControllers();
builder.Services.AddMemoryCache();  // This is important!
builder.Services.AddScoped<IJsonFileService, JsonFileService>();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddOptions<FileStorage>()
    .BindConfiguration(nameof(FileStorage));


//builder.Services.AddOutputCache(opciones =>
//{
//    //opciones.DefaultExpirationTimeSpan=TimeSpan.FromHours(1);
//    opciones.DefaultExpirationTimeSpan = TimeSpan.FromSeconds(15);
//    opciones.AddPolicy("15SecPolicy", p => p.Expire(TimeSpan.FromSeconds(15)));


//});



// Configure JWT authentication
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });

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
app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
