// ServiceExtensions.cs
using Microsoft.EntityFrameworkCore;
using TaskMarketplace.DAL;
using TaskMarketplace.DAL.Abstractions;
using TaskMarketplace.DAL.Repositories;
using TaskMarketplace.Service;
using TaskMarketplace.Service.Abstractions;

namespace TaskMarketplace.WebApi.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

   
        services.AddScoped<ITaskService, TaskService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}