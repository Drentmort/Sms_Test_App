using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Domain.Entities;

namespace SmsTest.Infrastructure.ExternalServices
{
	public class TestExternalService : IExternalService
	{
		private readonly ILogger<TestExternalService> _logger;
		private static readonly Random _random = new();

		public TestExternalService(ILogger<TestExternalService> logger)
		{
			_logger = logger;
		}

		public Task<List<Dish>> GetMenuAsync(bool withPrice, CancellationToken cancellationToken)
		{
			_logger.LogInformation("TestExternalService: Getting menu (withPrice: {WithPrice})", withPrice);

			var dishes = new List<Dish>
			{
                // Горячие блюда
                new Dish(
					id: "1",
					article: "HOT001",
					name: "Борщ с пампушками",
					price: 280.50m,
					isWeighted: false,
					fullPath: "Горячие блюда\\Супы"
				),
				new Dish(
					id: "2",
					article: "HOT002",
					name: "Солянка мясная",
					price: 320.00m,
					isWeighted: false,
					fullPath: "Горячие блюда\\Супы"
				),
				new Dish(
					id: "3",
					article: "MAIN001",
					name: "Котлета по-киевски",
					price: 450.00m,
					isWeighted: false,
					fullPath: "Горячие блюда\\Основные блюда"
				),
				new Dish(
					id: "4",
					article: "MAIN002",
					name: "Стейк Рибай",
					price: 1200.00m,
					isWeighted: true,
					fullPath: "Горячие блюда\\Основные блюда"
				),

                // Гарниры
                new Dish(
					id: "5",
					article: "SIDE001",
					name: "Картофель фри",
					price: 150.00m,
					isWeighted: false,
					fullPath: "Гарниры\\Картофель"
				),
				new Dish(
					id: "6",
					article: "SIDE002",
					name: "Овощи гриль",
					price: 180.00m,
					isWeighted: true,
					fullPath: "Гарниры\\Овощи"
				),

                // Салаты
                new Dish(
					id: "7",
					article: "SAL001",
					name: "Цезарь с курицей",
					price: 380.00m,
					isWeighted: false,
					fullPath: "Салаты\\Классические"
				),
				new Dish(
					id: "8",
					article: "SAL002",
					name: "Греческий салат",
					price: 290.00m,
					isWeighted: false,
					fullPath: "Салаты\\Овощные"
				),

                // Десерты
                new Dish(
					id: "9",
					article: "DES001",
					name: "Тирамису",
					price: 350.00m,
					isWeighted: false,
					fullPath: "Десерты\\Итальянские"
				),
				new Dish(
					id: "10",
					article: "DES002",
					name: "Чизкейк Нью-Йорк",
					price: 320.00m,
					isWeighted: false,
					fullPath: "Десерты\\Американские"
				),
				new Dish(
					id: "11",
					article: "DES003",
					name: "Медовик",
					price: 280.00m,
					isWeighted: true,
					fullPath: "Десерты\\Русские"
				),

                // Напитки
                new Dish(
					id: "12",
					article: "DRK001",
					name: "Кофе американо",
					price: 150.00m,
					isWeighted: false,
					fullPath: "Напитки\\Горячие"
				),
				new Dish(
					id: "13",
					article: "DRK002",
					name: "Чай черный с бергамотом",
					price: 120.00m,
					isWeighted: false,
					fullPath: "Напитки\\Горячие"
				),
				new Dish(
					id: "14",
					article: "DRK003",
					name: "Лимонад малина-мята",
					price: 200.00m,
					isWeighted: false,
					fullPath: "Напитки\\Прохладительные"
				),

                // Развесные продукты
                new Dish(
					id: "15",
					article: "WGH001",
					name: "Сыр пармезан (на развес)",
					price: 850.00m,
					isWeighted: true,
					fullPath: "Гастрономия\\Сыры"
				),
				new Dish(
					id: "16",
					article: "WGH002",
					name: "Колбаса сырокопченая",
					price: 650.00m,
					isWeighted: true,
					fullPath: "Гастрономия\\Колбасы"
				),
				new Dish(
					id: "17",
					article: "WGH003",
					name: "Конфеты шоколадные ассорти",
					price: 450.00m,
					isWeighted: true,
					fullPath: "Кондитерские изделия\\Конфеты"
				),

                // Суши и роллы
                new Dish(
					id: "18",
					article: "SUS001",
					name: "Филадельфия ролл",
					price: 480.00m,
					isWeighted: false,
					fullPath: "Японская кухня\\Роллы"
				),
				new Dish(
					id: "19",
					article: "SUS002",
					name: "Калифорния ролл",
					price: 450.00m,
					isWeighted: false,
					fullPath: "Японская кухня\\Роллы"
				),
				new Dish(
					id: "20",
					article: "SUS003",
					name: "Сет 'Самурай' (32 шт)",
					price: 1850.00m,
					isWeighted: false,
					fullPath: "Японская кухня\\Сеты"
				)
			};

			// Добавляем штрихкоды для некоторых блюд
			dishes[0].AddBarcode("4600000000011");
			dishes[0].AddBarcode("4600000000012");

			dishes[1].AddBarcode("4600000000021");

			dishes[3].AddBarcode("4600000000031");
			dishes[3].AddBarcode("4600000000032");
			dishes[3].AddBarcode("4600000000033");

			dishes[8].AddBarcode("4600000000041");

			dishes[14].AddBarcode("4600000000051");
			dishes[14].AddBarcode("4600000000052");

			dishes[16].AddBarcode("4600000000061");

			return Task.FromResult(dishes);
		}

		public Task<(bool Success, string ErrorMessage)> SendOrderAsync(Order order, CancellationToken cancellationToken)
		{
			_logger.LogInformation("TestExternalService: Sending order {OrderId} with {ItemCount} items",
				order.Id, order.Items.Count);

			// Логируем содержимое заказа
			foreach (var item in order.Items)
			{
				_logger.LogDebug("Order item: {DishName} (ID: {DishId}) - Quantity: {Quantity}, Price: {UnitPrice}, Total: {TotalPrice}",
					item.DishName, item.DishId, item.Quantity, item.UnitPrice, item.TotalPrice);
			}

			_logger.LogInformation("Order total amount: {TotalAmount:C}", order.TotalAmount);

			// Имитация различных сценариев ответа
			var scenarios = new[]
			{
                // Успешный ответ
                (Success: true, ErrorMessage: string.Empty),
                
                // Успешный ответ с предупреждением
                (Success: true, ErrorMessage: "Order processed with warnings: some items may be out of stock"),
                
                // Ошибка валидации
                (Success: false, ErrorMessage: "Validation failed: invalid order format"),
                
                // Ошибка сервера
                (Success: false, ErrorMessage: "Internal server error: database connection failed"),
                
                // Ошибка с конкретными позициями
                (Success: false, ErrorMessage: "Items out of stock: HOT001, SUS003"),
                
                // Таймаут
                (Success: false, ErrorMessage: "Request timeout: server not responding")
			};

			// Имитация задержки ответа (100-500ms)
			Thread.Sleep(_random.Next(100, 500));

			// Для демонстрации можно использовать разные сценарии:
			// - всегда успех: return Task.FromResult((true, string.Empty));
			// - случайный успех/ошибка:
			// var scenario = scenarios[_random.Next(scenarios.Length)];
			// return Task.FromResult(scenario);

			// Пока всегда возвращаем успех для тестирования
			return Task.FromResult((Success: true, ErrorMessage: string.Empty));
		}

		// Метод для имитации разных ответов в зависимости от суммы заказа
		public Task<(bool Success, string ErrorMessage)> SendOrderWithLogicAsync(Order order, CancellationToken cancellationToken)
		{
			if (order.TotalAmount > 10000)
			{
				return Task.FromResult((Success: false,
					ErrorMessage: "Order amount exceeds maximum limit of 10000"));
			}

			if (order.Items.Any(i => i.Quantity > 10))
			{
				return Task.FromResult((Success: false,
					ErrorMessage: "Maximum quantity per item is 10"));
			}

			if (order.Items.Count > 20)
			{
				return Task.FromResult((Success: false,
					ErrorMessage: "Maximum 20 items per order"));
			}

			// Успех в 80% случаев
			if (_random.NextDouble() < 0.8)
			{
				return Task.FromResult((Success: true, ErrorMessage: string.Empty));
			}

			return Task.FromResult((Success: false,
				ErrorMessage: "Random server error occurred"));
		}
	}
}