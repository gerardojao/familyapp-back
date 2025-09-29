using FamilyApp.Data;
using FamilyApp.Models;
using FamilyApp.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Servicios (antes de Build) --------------------
builder.Services.AddControllers();

// DB
builder.Services.AddDbContext<dbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Servicios propios
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRepository, Repository<dbContext>>();

// IEmailSender: registra SOLO una vez y SIEMPRE antes del Build.
// Por ahora usamos DevEmailSender tanto en dev como en prod.
// (Cuando tengas uno real, haz el if por environment aquí mismo.)
builder.Services.AddSingleton<IEmailSender, DevEmailSender>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Auth (JWT)
var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // en prod: true + HTTPS
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Sesión única (1 login por cuenta)
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                try
                {
                    var db = ctx.HttpContext.RequestServices.GetRequiredService<dbContext>();
                    var principal = ctx.Principal!;
                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? principal.FindFirstValue("sub");
                    var jti = principal.FindFirstValue("jti");

                    if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(jti))
                    {
                        ctx.Fail("Token inválido.");
                        return;
                    }

                    if (!int.TryParse(sub, out var userId))
                    {
                        ctx.Fail("Sub inválido."); return;
                    }

                    var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

                    if (user == null || user.ActiveSessionJti == null
                        || user.ActiveSessionJti.ToString() != jti
                        || (user.ActiveSessionExpiresAt.HasValue && user.ActiveSessionExpiresAt < DateTime.UtcNow))
                    {
                        ctx.Fail("Sesión no válida o caducada.");
                        return;
                    }
                }
                catch
                {
                    ctx.Fail("Error validando la sesión.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

// Swagger (antes de Build)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FamilyApp API", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce: Bearer {tu_token}"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [scheme] = Array.Empty<string>()
    });
});

// -------------------- Build --------------------
var app = builder.Build();

// Semilla de usuario admin (solo ejemplo)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<dbContext>();
    var pwd = scope.ServiceProvider.GetRequiredService<IPasswordService>();

    if (!db.Users.Any())
    {
        db.Users.Add(new AppUser
        {
            Email = "gerardojao@gmail.com",
            PasswordHash = pwd.Hash("Gjaoa*1970"),
            Role = "admin",
            IsActive = true
        });
        db.SaveChanges();
    }
}

// -------------------- Middleware (después de Build) --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("MyAllowSpecificOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
