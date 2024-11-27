// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace First;

internal class Program
{
    private static ServiceProvider? _serviceProvider;

    private static void RegisterServices()
    {
        _serviceProvider = new ServiceCollection()
            // .AddSingleton(new Application())
            .BuildServiceProvider(true);
    }

    private static void DisposeServices()
    {
        switch (_serviceProvider)
        {
            case null:
                return;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    public static async Task Main(string[] args)
    {
        #region Configuration

        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.Sources.Clear();

        IHostEnvironment env = builder.Environment;

        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        #endregion


        #region Logger

        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss.fff} {@l:u3}" +
                "{#if SourceContext is not null} ({SourceContext}){#end}] {@m}\n{@x}", theme: TemplateTheme.Literate))
            // additional config
            .CreateLogger();


        builder.Services.Configure<ServerConnection>(builder.Configuration);
        builder.Services.AddSerilog();
        builder.Services.AddHttpClient<PaperlessClient>(httpClient =>
        {
            ServerConnection serverConnection = new();
            builder.Configuration.GetSection(nameof(ServerConnection)).Bind(serverConnection);
        
            httpClient.BaseAddress = serverConnection.BaseAddress;
            var basicAuthenticationValue = 
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{serverConnection.User}:{serverConnection.Password}"));
            httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Basic", basicAuthenticationValue);
        });

        #endregion

        using var host = builder.Build();

        logger.Information("BEGIN");

        var serviceScope = host.Services.CreateScope();
        var serviceProvider = serviceScope.ServiceProvider;
        var client = serviceProvider.GetRequiredService<PaperlessClient>();
        var customFieldsAsync = await client.GetCustomFieldsAsync();

        logger.Information(customFieldsAsync);

        await host.RunAsync();

        // DisposeServices();
    }
}
