
using Languages.Registration.API.Configuration;

namespace Languages.Registration.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddApiBehavior();
            builder.Services.AddSwaggerConfiguration();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCorsPolicy();
            builder.Services.AddDependencies(builder.Configuration);
            builder.Services.AddIdentityConfiguration(builder.Configuration);
            builder.Services.AddAutheticationConfiguration(builder.Configuration);

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseCors(app.Environment.EnvironmentName);
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
