using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SmsWpfApp
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly EnvironmentService _environmentService;
		private readonly ILogger<MainWindow> _logger;
		private ObservableCollection<VariableModel> _variables;

		public ObservableCollection<VariableModel> Variables
		{
			get => _variables;
			set
			{
				_variables = value;
				OnPropertyChanged();
			}
		}

		public MainWindow(EnvironmentService environmentService, ILogger<MainWindow> logger)
		{
			InitializeComponent();

			_environmentService = environmentService;
			_logger = logger;

			DataContext = this;
			LoadVariables();
		}

		private void LoadVariables()
		{
			try
			{
				var list = _environmentService.GetVariables();
				Variables = new ObservableCollection<VariableModel>(list);
				_logger.LogInformation("Загружено {Count} переменных среды", list.Count);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при загрузке переменных");
				MessageBox.Show($"Ошибка при загрузке переменных: {ex.Message}",
					"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var modifiedVariables = Variables.Where(v =>
					v.Value != Environment.GetEnvironmentVariable(v.Name, EnvironmentVariableTarget.User) ||
					!string.IsNullOrEmpty(v.Comment)).ToList();

				if (!modifiedVariables.Any())
				{
					MessageBox.Show("Нет изменений для сохранения", "Информация",
						MessageBoxButton.OK, MessageBoxImage.Information);
					return;
				}

				_environmentService.SaveVariables(Variables.ToList());

				_logger.LogInformation("Сохранено {Count} изменений", modifiedVariables.Count);
				MessageBox.Show("Изменения успешно сохранены", "Успех",
					MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Ошибка при сохранении переменных");
				MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
					"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			LoadVariables();
			_logger.LogInformation("Список переменных обновлен");
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class VariableModel : INotifyPropertyChanged
	{
		private string _value;
		private string _comment;

		public string Name { get; set; }

		public string Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_value = value;
					OnPropertyChanged();
				}
			}
		}

		public string Comment
		{
			get => _comment;
			set
			{
				if (_comment != value)
				{
					_comment = value;
					OnPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}