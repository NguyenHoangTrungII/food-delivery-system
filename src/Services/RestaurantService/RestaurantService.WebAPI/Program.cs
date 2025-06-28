using FoodDeliverySystem.Common;
using FoodDeliverySystem.DataAccess.Extensions;
using Microsoft.OpenApi.Models;
using FoodDeliverySystem.Common.Authorization.Middlewares;
using FoodDeliverySystem.Common.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using RestaurantService.Infrastructure.Configurations;
using UserProfileService.Domain.SeedWork;
//using RestaurantService.Infrastructure.Geo;
using FoodDeliverySystem.Common.Geo.Extensions;
using static FoodDeliverySystem.Common.Geo.Extensions.ServiceCollectionExtensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
//builder.Services.UseDAL<RestaurantDbContext>(builder.Configuration);
builder.Services.UseDAL<RestaurantDbContext>(builder.Configuration, "Database:Services");




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

builder.Services.AddCommonMediatRAndValidation(typeof(Program).Assembly);

// Add PostGIS GeoDatabaseProvider
//builder.Services.AddSingleton<FoodDeliverySystem.Common.Geo.Interfaces.IGeoDatabaseProvider>(sp =>
//    new PostgisGeoDatabaseProvider(builder.Configuration["PostGIS:ConnectionString"]));

//Add GeoDistance with PostGIS engine
//builder.Services.AddGeoDistance(
//    builder.Configuration,
//    "PostGIS",
//   postgisTableName: builder.Configuration["PostGIS:TableName"] ?? "restaurants");

builder.Services.AddGeoDistance(options =>
{

    var restaurantConnStr = builder.Configuration.GetSection("Database:Services:Restaurant:ConnectionString").Value;
    options.PostGIS = new GeoOptions.PostGISConfig
    {
        ConnectionString = restaurantConnStr,
        TableName = "restaurants"
    };

    var configuration = builder.Configuration.GetSection("Geo");
    options.Engine = new GeoOptions.GeoEngineConfig
    {
        Engine = configuration["Engine"] ?? "OSRM" // Mặc định OSRM
    };

    options.OSRM = new GeoOptions.OSRMConfig
    {
        Url = configuration["OSRM:Url"] ?? "http://localhost:5000/",
        Profile = configuration["OSRM:Profile"] ?? "driving",
        DataPath = configuration["OSRM:DataPath"] // Đường dẫn đến file OSM nếu cần
    };
});


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

builder.Services.AddHttpClient<WeatherService>(client =>
    client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5/"))
.AddTypedClient((httpClient, sp) => new WeatherService(
    httpClient,
    builder.Configuration["Weather:ApiKey"], // 👈 Hoặc sp.GetRequiredService<IConfiguration>()
    sp.GetRequiredService<ILogger<WeatherService>>()));


// Add Authorization
builder.Services.AddAuthorization();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Restaurant API", Version = "v1" });

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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant  API V1");
    c.RoutePrefix = string.Empty; 
});


app.UseAuthentication();
app.UseServiceAuthorization();
app.UseAuthorization();
app.MapControllers();

app.Run();



//using AutoMapper;
//using FoodDeliverySystem.Common;
//using FoodDeliverySystem.Common.Caching;
//using FoodDeliverySystem.Common.Geo.Extensions;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.DataAccess.Extensions;
//using MediatR;
//using Microsoft.Extensions.Configuration;
//using RestaurantService.Application.Dtos;
//using RestaurantService.Application.Handlers;
//using RestaurantService.Domain.Entities;
//using RestaurantService.Infrastructure.Configurations;
//using RestaurantService.Infrastructure.Geo;

//var builder = WebApplication.CreateBuilder(args);

//// Add services
//builder.Services.AddControllers();

//// Add Common services (Logging, Caching, ApiResponse)
//builder.Services.AddCommonServices(builder.Configuration);

//// Add MediatR
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FindRestaurantsNearbyHandler).Assembly));

//// Add AutoMapper
//builder.Services.AddAutoMapper(cfg =>
//{
//    cfg.CreateMap<Restaurant, RestaurantDto>();
//});

//// Add DAL with RestaurantDbContext
//builder.Services.UseDAL<RestaurantDbContext>(builder.Configuration, "Database:Services");

//// Add PostGIS GeoDatabaseProvider
//builder.Services.AddSingleton<IGeoDatabaseProvider>(sp =>
//    new PostgisGeoDatabaseProvider(builder.Configuration["PostGIS:ConnectionString"]));

//// Add Redis caching
//builder.Services.AddRedisCaching(builder.Configuration);

//// Add GeoDistance with PostGIS engine
//builder.Services.AddGeoDistance(
//    builder.Configuration,
//    "PostGIS",
//    postgisTableName: builder.Configuration["PostGIS:TableName"] ?? "restaurants");

//var app = builder.Build();

//// Configure pipeline
//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();