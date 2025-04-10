using Azure.Storage.Blobs;
using English.Registration.API.Factories;
using English.Registration.API.Repositories;
using English.Registration.API.Repositories.Contracts;
using English.Registration.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using Store.MongoDb.Identity.Extensions;
using Store.MongoDb.Identity.Models;
using System.Text;

namespace English.Registration.API.Configuration
{
    public static class StartupExtensions
    {
        public static void AddApiBehavior(this IServiceCollection services)
        {
            var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
            ConventionRegistry.Register("CamelCaseElementNameConvention", conventionPack, t => true);

            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
        }

        public static void AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            services.AddSingleton<MongoDbCollectionFactory>();

            services.AddScoped<IAppUserRepository, AppUserRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();

            #region BlobStorage
            services.AddSingleton(serviceProvider =>
            {
                var blobServiceClient = new BlobServiceClient(configuration["BlobStorage:ConnectionString"]);
                return blobServiceClient.GetBlobContainerClient(configuration["BlobStorage:ContainerName"]);
            });
            services.AddSingleton<IBlobStorageService, BlobStorageService>();
            #endregion
        }

        public static void AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(setup =>
            {
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name = "JWT Authentication",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                setup.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });
        }

        public static void AddIdentityConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<MongoUser, MongoRole>(o =>
            {
                o.Password.RequiredUniqueChars = 0;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                o.Lockout.MaxFailedAccessAttempts = 6;
                o.Lockout.AllowedForNewUsers = true;
            })
                .AddDefaultTokenProviders()
                .AddIdentityMongoDbStores<MongoUser, MongoRole, ObjectId>(o =>
                {
                    o.ConnectionString = configuration.GetConnectionString("MongoDb")
                        ?? throw new InvalidOperationException("ConnectionString not defined in 'app settings'");
                    o.UsersCollection = "aspnetusers";
                });
        }

        public static void AddAutheticationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ??
                throw new InvalidOperationException("Jwt not defined in 'app settings'"); ;

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwt.SecretKey)),
                    ValidAudience = jwt.Audience,
                    ValidIssuer = jwt.Issuer,
                };
            });
        }

        public static void AddCorsPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("Development", policy =>
                {
                    policy.AllowAnyOrigin();
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });

                options.AddPolicy("Production", policy =>
                {
                    var allowedOrigins = configuration["AllowedOrigins"]?.Split(";") ?? [];
                    policy.WithOrigins(allowedOrigins);
                    policy.AllowAnyHeader();
                    policy.AllowAnyMethod();
                });
            });
        }
    }
}
