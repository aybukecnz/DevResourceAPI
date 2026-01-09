using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevResourceAPI.Data;
using DevResourceAPI.Services;
using OpenApi = Microsoft.OpenApi.Models; 

var builder = WebApplication.CreateBuilder(args);

// --- 1. AUTHENTICATION (JWT) ---
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

// VERİTABANI 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
// Servisleri kaydet
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
//builder.Services.AddScoped<IAuthService, AuthService>();
// CONTROLLERS 
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddScoped<AuthService>(); // Senin servisin

//SWAGGER (HATASIZ KONFİGÜRASYON)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // JWT Tanımı
    c.AddSecurityDefinition("Bearer", new OpenApi.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = OpenApi.ParameterLocation.Header,
        Description = "JWT Token girin."
    });

    // API Key Tanımı
    c.AddSecurityDefinition("ApiKey", new OpenApi.OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = OpenApi.SecuritySchemeType.ApiKey,
        In = OpenApi.ParameterLocation.Header,
        Description = "Uygulama anahtarını (X-Api-Key) girin."
    });

    // Kilitleri Aktif Eden Kısım
    c.OperationFilter<DevResourceAPI.SwaggerSecurityFilter>();
});

var app = builder.Build();

// PIPELINE AYARLARI 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Sıralama Önemli: Önce Authentication, Sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();