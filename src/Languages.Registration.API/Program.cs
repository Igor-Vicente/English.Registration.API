
using Languages.Registration.API.Configuration;
using Serilog;

namespace Languages.Registration.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddHealthChecks();
            builder.Services.AddControllers();
            builder.Services.AddApiBehavior();
            builder.Services.AddSwaggerConfig();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCorsPolicy(builder.Configuration);
            builder.Services.AddDependencies(builder.Configuration);
            builder.Services.AddIdentityConfig(builder.Configuration);
            builder.Services.AddAutheticationConfig(builder.Configuration);
            builder.Host.UseSerilog((ctx, con) =>
                con.ReadFrom.Configuration(ctx.Configuration));

            var app = builder.Build();

            app.UseMiddleware<MiddlewareException>();
            app.UseSerilogRequestLogging();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(app.Environment.EnvironmentName);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.UseHealthChecks("/health");

            app.Run();
        }
    }
}
