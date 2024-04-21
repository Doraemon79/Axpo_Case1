using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using ReportGeneratorLogic.Services;
using ReportGeneratorLogic.Services.Interfaces;
using CsvHelper.Configuration;
using System.Globalization;
using Serilog;

namespace ReportGenerator
{
    class Program
    {
        private static IServiceProvider ServiceProvider;


        static async Task Main(string[] args)
        {
            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/reportgenerator.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Starting up the service...");
                ServiceProvider = ConfigureServices(args);
                if (!ValidateConfiguration())
                {
                    Log.Fatal("Configuration validation failed.");
                    return; // Exit the application if configuration is invalid
                }
                await RunScheduledTasks();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
       
        private static bool ValidateConfiguration()
        {
            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            string outputPath = configuration["OutputFolderPath"];
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Console.WriteLine("Output path is not specified in the configuration.");
                return false;
            }

            if (!Directory.Exists(outputPath))
            {
                try
                {
                    Directory.CreateDirectory(outputPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create directory at '{outputPath}': {ex.Message}");
                    return false;
                }
            }

            if (!int.TryParse(configuration["IntervalMinutes"], out int interval) || interval < 1)
            {
                Console.WriteLine("Interval must be an integer of 2 or higher.");
                return false;
            }

            return true;
        }

        private static IServiceProvider ConfigureServices(string[] args)
        {
            var services = new ServiceCollection();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
                .AddCommandLine(args, new Dictionary<string, string> {
                    {"--outputPath", "OutputFolderPath"},
                    {"--interval", "IntervalMinutes"},
                    {"--timezone", "TraderLocation" }
                })
                .Build();

            string timeZoneCode = configuration["TraderLocation"]!;

            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IPowerTradeService, PowerTradeService>();
            services.AddSingleton<ITradeAggregationService>(_ => new TradeAggregationService(timeZoneCode));
            services.AddSingleton<ICsvWriter, CsvWriterService>();
            services.AddSingleton(new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });

            return services.BuildServiceProvider();
        }

        private static async Task RunScheduledTasks()
        {
            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            var intervalMinutes = configuration.GetValue<int>("IntervalMinutes");
            var interval = TimeSpan.FromMinutes(intervalMinutes);

            while (true)
            {
                await GenerateAndSaveReport();
                await Task.Delay(interval);
            }
        }

        private static async Task GenerateAndSaveReport()
        {
            int retryAttempts = 0;
            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            int maxRetries;
            int.TryParse(configuration["MaxRetries"], out maxRetries);

            while (retryAttempts < maxRetries)
            {
                try
                {
                    var tradeAggregationService = ServiceProvider.GetRequiredService<ITradeAggregationService>();
                    var powerTradeService = ServiceProvider.GetRequiredService<IPowerTradeService>();
                    var csvWriter = ServiceProvider.GetRequiredService<ICsvWriter>();

                    DateTime followingDay = DateTime.UtcNow.AddDays(1);
                    var trades = await powerTradeService.GetTradesAsync(followingDay);
                    var records = tradeAggregationService.AggregateTrades(trades, followingDay);

                    
                    string reportDirectoryPath = configuration["OutputFolderPath"];
                    if (!Directory.Exists(reportDirectoryPath))
                    {
                        Directory.CreateDirectory(reportDirectoryPath);
                    }

                    string filePath = $"{reportDirectoryPath}PowerPosition_{DateTime.UtcNow:yyyyMMdd}_{DateTime.UtcNow:HHmm}.csv";
                    csvWriter.WriteCsv(records, filePath);

                    Log.Information("Report generated and saved to: {FilePath}", filePath);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to generate and save report on attempt {RetryAttempt}/{MaxRetries}", retryAttempts + 1, maxRetries);
                    retryAttempts++;
                    if (retryAttempts >= maxRetries)
                    {
                        Log.Error("All attempts failed; the operation will not be retried further for this specific interval.");
                    }
                    await Task.Delay(5000); // Wait 5 seconds before retrying
                }
            }
        }

    }
}
