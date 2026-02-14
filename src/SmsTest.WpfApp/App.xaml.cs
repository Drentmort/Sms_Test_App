using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Windows;

namespace SmsWpfApp
{
	public partial class App : Application
	{
		private IServiceProvider _serviceProvider;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			_serviceProvider = serviceCollection.BuildServiceProvider();

			var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
			mainWindow.Show();
		}

		private void ConfigureServices(IServiceCollection services)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			services.AddSingleton<IConfiguration>(configuration);
			services.Configure<AppSettings>(configuration);

			var logFileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				$"test-sms-wpf-app-{DateTime.Now:yyyyMMdd}.log");

			services.AddLogging(builder =>
			{
				builder.ClearProviders();
				builder.AddProvider(new FileLoggerProvider(logFileName));
				builder.AddConsole();
			});

			services.AddSingleton<EnvironmentService>();
			services.AddSingleton<MainWindow>();
		}
	}

	public class AppSettings
	{
		public string[] EnvironmentVariables { get; set; }
	}
}