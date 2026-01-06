using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Data;
using DevResourceAPI.Models;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.JsonPatch;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Controllers güncellendi .AddNewtonsoftJson() eklendi 
builder.Services.AddControllers().AddNewtonsoftJson();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DevResource API",
        Version = "v1",
        Description = "Yazılımcılar için kaynak kütüphanesi API",
        Contact = new OpenApiContact
        {
            Name = "Aybüke Canöz",
            Url = new Uri("https://github.com/aybukecnz")
        }
    });
});

var app = builder.Build();

// Global Exception
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        var response = new ErrorResponse
        {
            StatusCode = 500,
            Message = "Beklenmedik bir sunucu hatası oluştu.",
            Detailed = app.Environment.IsDevelopment() ? exception?.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
