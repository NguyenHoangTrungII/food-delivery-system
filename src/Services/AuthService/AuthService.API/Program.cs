using AuthService.Infrastructure.Data;
using AuthService.Infrastructure.IRepositories;
using AuthService.Infrastructure.Repositories;
using AuthService.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using EffiAP.Domain.SeedWork;
using System.Reflection;
using FluentValidation;
using AuthService.Application.Pipelines;
using FluentValidation.AspNetCore;
using FoodDeliverySystem.Common.Extensions;
using FoodDeliverySystem.Common.Authorization.Middlewares;
using FoodDeliverySystem.Common.Caching;
using FoodDeliverySystem.Common;
using AuthService.Application.Services.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//Use Scrutor and register service lifetime for Interface
builder.Services.Scan(scan => scan
    .FromAssemblies(AssemblyHelper.GetAllAssemblies())
    //.FromAssemblyOf<IInjectableService>()
    .AddClasses(classes => classes.AssignableTo<ITransientService>())
    .AsImplementedInterfaces()
    .WithTransientLifetime()

    .AddClasses(classes => classes.AssignableTo<IScopedService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime()

    .AddClasses(classes => classes.AssignableTo<ISingletonService>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime()

    .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);


builder.Services.AddCommonServices(builder.Configuration);
builder.Services.AddSingleton<PermissionChangePublisher>();





// Đăng ký FluentValidation
//builder.Services.AddFluentValidationAutoValidation();
//builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// Add MediatR
// Trong builder.Services.AddMediatR
//builder.Services.AddMediatR(cfg =>
//{
//    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
//    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
//});

builder.Services.AddCommonMediatRAndValidation(typeof(Program).Assembly);


// Add Authentication with JWT
var jwtSecret = builder.Configuration["Jwt:Secret"];
var key = Encoding.UTF8.GetBytes(jwtSecret);
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // 👈 xóa mặc định
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        //ValidIssuer = "AuthService",
        ValidIssuer = "FoodDeliverySystem",
        ValidAudience = "FoodDeliverySystem",
        NameClaimType = ClaimTypes.NameIdentifier, 
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };


    // Bắt lỗi và in ra lý do
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"❌ Token authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API V1");
    c.RoutePrefix = string.Empty; // Truy cập Swagger tại root (http://localhost:5001/)
});


app.UseAuthentication();
app.UseServiceAuthorization();
app.UseAuthorization();
app.MapControllers();

app.Run();