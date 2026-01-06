using Microsoft.AspNetCore.Diagnostics;
using DevResourceAPI.Models;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Data;
var builder = WebApplication.CreateBuilder(args);

// Veritabanı servisini ekle
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// GLOBAL HATA YÖNETİMİ 
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500; // İç Sunucu Hatası
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var response = new ErrorResponse
        {
            StatusCode = context.Response.StatusCode,
            Message = "Beklenmedik bir sunucu hatası oluştu.",
            Detailed = app.Environment.IsDevelopment() ? exception?.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
