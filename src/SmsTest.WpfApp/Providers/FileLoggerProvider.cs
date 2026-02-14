using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace SmsWpfApp
{
	public class FileLoggerProvider : ILoggerProvider
	{
		private readonly string _filePath;

		public FileLoggerProvider(string filePath)
		{
			_filePath = filePath;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return new FileLogger(_filePath);
		}

		public void Dispose()
		{
		}
	}

	public class FileLogger : ILogger
	{
		private readonly string _filePath;
		private static readonly object _lock = new object();

		public FileLogger(string filePath)
		{
			_filePath = filePath;
		}

		public IDisposable BeginScope<TState>(TState state)
		{
			return null;
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return true;
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
				return;

			var message = formatter(state, exception);
			var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{logLevel}] {message}";

			if (exception != null)
			{
				logEntry += $"\nException: {exception}";
			}

			lock (_lock)
			{
				File.AppendAllText(_filePath, logEntry + Environment.NewLine);
			}
		}
	}
}