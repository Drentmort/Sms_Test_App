using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SmsWpfApp
{
	public class EnvironmentService
	{
		private readonly ILogger<EnvironmentService> _logger;
		private readonly string[] _variableNames;
		private readonly string _commentsFilePath;

		public EnvironmentService(IOptions<AppSettings> settings, ILogger<EnvironmentService> logger)
		{
			_variableNames = settings.Value.EnvironmentVariables;
			_logger = logger;
			_commentsFilePath = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				"variable_comments.json");
		}

		public List<VariableModel> GetVariables()
		{
			var comments = LoadComments();
			var list = new List<VariableModel>();

			foreach (var name in _variableNames)
			{
				var value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
				var model = new VariableModel
				{
					Name = name,
					Value = value ?? string.Empty
				};

				if (comments.TryGetValue(name, out string comment))
				{
					model.Comment = comment;
				}

				list.Add(model);
				_logger.LogDebug("Прочитана переменная {Name} = {Value}, комментарий: {Comment}",
					name, value ?? "(не задана)", model.Comment ?? "(нет)");
			}

			return list;
		}

		public void SaveVariables(List<VariableModel> variables)
		{
			var comments = new Dictionary<string, string>();

			foreach (var variable in variables)
			{
				var oldValue = Environment.GetEnvironmentVariable(variable.Name, EnvironmentVariableTarget.User);

				if (oldValue != variable.Value)
				{
					Environment.SetEnvironmentVariable(variable.Name, variable.Value, EnvironmentVariableTarget.User);
					_logger.LogInformation("Переменная '{Name}' изменена: '{OldValue}' -> '{NewValue}'",
						variable.Name, oldValue ?? "(null)", variable.Value ?? "(null)");
				}

				if (!string.IsNullOrEmpty(variable.Comment))
				{
					comments[variable.Name] = variable.Comment;
				}
				else if (comments.ContainsKey(variable.Name))
				{
					comments.Remove(variable.Name);
				}
			}

			SaveComments(comments);
		}

		private Dictionary<string, string> LoadComments()
		{
			try
			{
				if (File.Exists(_commentsFilePath))
				{
					string json = File.ReadAllText(_commentsFilePath);
					return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
						   ?? new Dictionary<string, string>();
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при загрузке комментариев");
			}

			return new Dictionary<string, string>();
		}

		private void SaveComments(Dictionary<string, string> comments)
		{
			try
			{
				string json = JsonSerializer.Serialize(comments, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(_commentsFilePath, json);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении комментариев");
			}
		}
	}
}