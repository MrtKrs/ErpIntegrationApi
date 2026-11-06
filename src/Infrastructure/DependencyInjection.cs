using System.Text;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Notifications;
using Infrastructure.Authentication;
using Infrastructure.Authorization;
using Infrastructure.Database;
//using Infrastructure.DomainEvents;
using Infrastructure.Notifications;
using Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddAuthenticationInternal()
            .AddAuthorizationInternal();

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        //services.AddTransient<IDomainEventsDispatcher, DomainEventsDispatcher>();

        services.AddScoped<IEmailSender, LoggingEmailSender>();

#pragma warning disable EXTEXP0018 // HybridCache is released; the API is stable in .NET 10.
        services.AddHybridCache();
#pragma warning restore EXTEXP0018

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database") 
            ?? throw new InvalidOperationException("Connection string 'Database' not found.");


        // services.AddDbContext<ApplicationDbContext>(
        //     options => options
        //         .UseNpgsql(connectionString, npgsqlOptions =>
        //             npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default))
        //         .UseSnakeCaseNamingConvention());

        // services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory( connectionString));
            //configuration.GetConnectionString("Database")?? throw new InvalidOperationException("Connection string 'Database' not found.")));

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO:
        _ = configuration;
        services.AddHealthChecks();

        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services)
    {
        services.AddScoped<IApiKeyValidationService, ApiKeyValidationService>();

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = ApiKeyAuthOptions.SchemeName;
            options.DefaultAuthenticateScheme = ApiKeyAuthOptions.SchemeName;
            options.DefaultChallengeScheme = ApiKeyAuthOptions.SchemeName;
        })
        .AddScheme<ApiKeyAuthOptions, ApiKeyAuthHandler> (ApiKeyAuthOptions.SchemeName, null);

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        
        services.AddSingleton<IPasswordHasher, PasswordHasher>(); //REMOVE 
        services.AddSingleton<ITokenProvider, TokenProvider>(); //REMOVE
        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        services.AddScoped<PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
