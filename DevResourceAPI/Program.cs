using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DevResourceAPI; // Filtre dosyasını görmek için
using DevResourceAPI.Data;
using DevResourceAPI.Services;
using OpenApi = Microsoft.OpenApi.Models;
using DevResourceAPI.Models; // User modeli için
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// --- VERİTABANI ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddIdentityApiEndpoints<User>(options => 
{
    options.User.RequireUniqueEmail = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole<int>>()
.AddEntityFrameworkStores<AppDbContext>();
// --- SERVİSLER ---
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISocialService, SocialService>();
builder.Services.AddScoped<IUserService, UserService>(); 

// --- CONTROLLERS ---
builder.Services.AddControllers().AddNewtonsoftJson();

// --- AUTHENTICATION ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
// --- SWAGGER AYARLARI (DÜZELTİLEN KISIM) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApi.OpenApiInfo { Title = "DevResourceAPI", Version = "v1" });

    // 1. JWT Kutusu Tanımı (Ana Butonda Görünecek)
    c.AddSecurityDefinition("Bearer", new OpenApi.OpenApiSecurityScheme
    {
        Description = "JWT Token buraya girilecek (Bearer eyJ...)",
        Name = "Authorization",
        In = OpenApi.ParameterLocation.Header,
        Type = OpenApi.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // 2. API Key Kutusu Tanımı (Ana Butonda Görünecek - İstediğin Gibi)
    c.AddSecurityDefinition("ApiKey", new OpenApi.OpenApiSecurityScheme
    {
        Description = "API Key girişi",
        Name = "X-Api-Key",
        In = OpenApi.ParameterLocation.Header,
        Type = OpenApi.SecuritySchemeType.ApiKey
    });

    // 3. AKILLI FİLTRE (Global zorunluluk yerine bunu kullanıyoruz!)
    // Bu satır sayesinde kilit ikonu sadece [Authorize] olanlarda çıkacak.
    c.OperationFilter<SwaggerFileOperationFilter>();
});

var app = builder.Build();
// Bu satır, hava yastığını devreye sokar (globalexceptionmiddleware'i)
app.UseMiddleware<DevResourceAPI.Middlewares.GlobalExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// 1. ÖNCE KİMLİK DOĞRULAMA (Sen kimsin?)
app.UseAuthentication();
// 2. YENİ GÜVENLİK KATMANI (Anahtarın var mı?)
app.UseMiddleware<DevResourceAPI.Middleware.ApiKeyMiddleware>();
// 3. SONRA YETKİLENDİRME (Yetkin var mı?)
app.UseAuthorization();
app.MapControllers();


if (app.Environment.IsDevelopment())
{
    await DevResourceAPI.Data.DbSeeder.SeedData(app, builder.Configuration);
}
app.Run();