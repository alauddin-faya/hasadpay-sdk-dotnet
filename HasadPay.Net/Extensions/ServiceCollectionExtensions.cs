using HasadPay.Net.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HasadPay.Net.Extensions;

/// <summary>
/// Extension methods for registering HasadPay services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers HasadPay SDK client and services using a configuration delegate.
    /// </summary>
    public static IServiceCollection AddHasadPay(
        this IServiceCollection services,
        Action<HasadPayOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        RegisterCoreServices(services);

        return services;
    }

    /// <summary>
    /// Registers HasadPay SDK client and services bound from an <see cref="IConfiguration"/> section.
    /// </summary>
    public static IServiceCollection AddHasadPay(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = HasadPayOptions.SectionName)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(sectionName);
        services.Configure<HasadPayOptions>(section);
        RegisterCoreServices(services);

        return services;
    }

    private static void RegisterCoreServices(IServiceCollection services)
    {
        // Register HttpClient for HasadPay
        services.AddHttpClient<IHasadPayClient, HasadPayClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<HasadPayOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            }
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        });

        // Register domain services for direct dependency injection
        services.AddTransient<ITransactionsService>(sp => sp.GetRequiredService<IHasadPayClient>().Transactions);
        services.AddTransient<IInvoicesService>(sp => sp.GetRequiredService<IHasadPayClient>().Invoices);
        services.AddTransient<IPaymentMethodsService>(sp => sp.GetRequiredService<IHasadPayClient>().Methods);
    }
}
