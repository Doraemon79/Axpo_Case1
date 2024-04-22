using CsvHelper.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReportGeneratorLogic.Services;
using ReportGeneratorLogic.Services.Interfaces;
using Serilog;
using System.Globalization;

namespace ReportGenerator
{
    class Program
    {
        private static IServiceProvider? ServiceProvider;

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
                    return; // Exit the application if Configuration is invalid
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
                Log.Error("Output path is not specified in the configuration.");
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
                    Log.Error("Failed to create directory at '{OutputPath}': {ExceptionMessage}", outputPath, ex.Message);
                    return false;
                }
            }

            if (!int.TryParse(configuration["IntervalMinutes"], out int interval) || interval < 1)
            {
                Log.Error("Interval must be an integer of 1 or higher.");
                return false;
            }

            string inputDate = configuration["ReportDate"];
            if (!DateTime.TryParseExact(inputDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime reportDate))
            {
                Log.Error("Invalid date format. Please ensure the date is in ISO 8601 format (YYYY-MM-DD). Received date: {InputDate}", inputDate);
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

            services.AddSingleton(configuration);
            services.AddTransient<IPowerTradeService, PowerTradeService>();
            services.AddTransient<ITradeAggregationService>(_ => new TradeAggregationService(timeZoneCode));
            services.AddTransient<ICsvWriter, CsvWriterService>();
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
            var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            int retryAttempts = 0;
            int maxRetries;
            int.TryParse(configuration["MaxRetries"], out maxRetries);

            // Getting the previously validated report date from the configuration
            DateTime.TryParseExact(configuration["ReportDate"], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime reportDate);

            while (retryAttempts < maxRetries)
            {
                try
                {
                    var tradeAggregationService = ServiceProvider.GetRequiredService<ITradeAggregationService>();
                    var powerTradeService = ServiceProvider.GetRequiredService<IPowerTradeService>();
                    var csvWriter = ServiceProvider.GetRequiredService<ICsvWriter>();


                    var trades = await powerTradeService.GetTradesAsync(reportDate.AddDays(1));
                    var records = tradeAggregationService.AggregateTrades(trades, reportDate.AddDays(1));

                    string reportDirectoryPath = configuration["OutputFolderPath"];
                    if (!Directory.Exists(reportDirectoryPath))
                    {
                        _ = Directory.CreateDirectory(reportDirectoryPath);
                    }

                    string filePath = $"{reportDirectoryPath}PowerPosition_{reportDate:yyyyMMdd}_{DateTime.UtcNow:HHmm}.csv";
                    using (var writer = new StreamWriter(filePath))
                    using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }))
                    {
                        csv.WriteRecords(records); // Writing the records to the CSV file
                    }

                    Log.Information("Report generated and saved to: {FilePath}", filePath);
                    return; // Exit the method after successful execution
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to generate and save report on attempt {RetryAttempt}/{MaxRetries}", retryAttempts + 1, maxRetries);
                    retryAttempts++;
                    if (retryAttempts >= maxRetries)
                    {
                        Log.Error("All attempts failed; the operation will not be retried further for this specific interval.");
                        break; // Exit the loop after the last attempt
                    }
                    await Task.Delay(5000); // Wait for 5 seconds before the next retry
                }
            }
        }
    }
}
