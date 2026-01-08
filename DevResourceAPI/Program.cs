using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using DevResourceAPI.Data;
using DevResourceAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Swashbuckle.AspNetCore.Filters;


var builder = WebApplication.CreateBuilder(args);

// --- 1. AUTHENTICATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// --- 2. VERİTABANI ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --- 3. SERVİSLER ---
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<AuthService>();

// --- 4. SWAGGER (HATALARI ÖNLEYEN TAM ADRESLİ KOD) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Başına 'global::' ekleyerek "Dışarıdaki kütüphaneye git, benim klasörüme bakma" diyoruz.
    options.AddSecurityDefinition("oauth2", new global::Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT için Bearer şeması kullanılmalıdır. Örnek: 'Bearer {token}'",
        In = global::Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = global::Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });

    options.OperationFilter<global::Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
});

var app = builder.Build();

// --- 5. GLOBAL HATA YÖNETİMİ ---
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = feature?.Error;

        var response = new 
        {
            StatusCode = 500,
            Message = "Beklenmedik bir sunucu hatası oluştu.",
            Detailed = app.Environment.IsDevelopment() ? exception?.Message : null
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

// --- 6. HTTP PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers();

app.Run();