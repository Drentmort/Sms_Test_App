using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SmsTest.Application;
using SmsTest.Application.Common.Interfaces;
using SmsTest.Application.Queries;
using SmsTest.Infrastructure;
using SmsTest.Infrastructure.Services;
using Spectre.Console;
using MediatR;
using SmsTest.Application.Commands;

namespace SmsTest.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"logs/test-sms-console-app-{DateTime.Now:yyyyMMdd}.log")
                .CreateLogger();

            try
            {
                Log.Information("Starting application...");
                
                var host = CreateHostBuilder(args).Build();
                
                using var scope = host.Services.CreateScope();
                var services = scope.ServiceProvider;
                
                await RunApplicationAsync(services);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddApplication();
                    services.AddInfrastructure(context.Configuration);
                });

        static async Task RunApplicationAsync(IServiceProvider services)
        {
            var mediator = services.GetRequiredService<IMediator>();
            var logger = services.GetRequiredService<ILogger<Program>>();
            var databaseInitializer = services.GetRequiredService<IDatabaseInitializer>();
            var dishRepository = services.GetRequiredService<IDishRepository>();
            
            try
            {
                AnsiConsole.MarkupLine("[green]1. Инициализация базы данных...[/]");
                await databaseInitializer.InitializeAsync(CancellationToken.None);
                AnsiConsole.MarkupLine("[green]✓ База данных инициализирована[/]\n");
                
                AnsiConsole.MarkupLine("[green]2. Получение меню с сервера...[/]");
                var menuResult = await mediator.Send(new GetMenuQuery { WithPrice = true });

                if (!menuResult.Success)
                {
                    AnsiConsole.MarkupLine($"[red]Ошибка: {menuResult.ErrorMessage}[/]");
                    return;
                }
                
                var dishes = menuResult.Data ?? new List<DishDto>();
                AnsiConsole.MarkupLine($"[green]✓ Получено {dishes.Count} блюд[/]\n");
                
                var existingDishes = await dishRepository.GetAllAsync(CancellationToken.None);
                AnsiConsole.MarkupLine("[green]3. Список блюд:[/]");
                
                var table = new Table();
                table.AddColumn("Название");
                table.AddColumn("Артикул");
                table.AddColumn("Цена");
                table.AddColumn("Вес/Шт");
                
                foreach (var dish in dishes)
                {
                    table.AddRow(
                        dish.Name.EscapeMarkup(),
                        dish.Article.EscapeMarkup(),
                        $"{dish.Price:C}",
                        dish.IsWeighted ? "Вес" : "Шт"
                    );
                }
                
                AnsiConsole.Write(table);
                AnsiConsole.WriteLine();
                
                var order = new List<OrderItemDto>();
                
                bool validInput = false;
                while (!validInput)
                {
                    AnsiConsole.MarkupLine("[yellow]4. Ввод заказа[/]");
                    AnsiConsole.MarkupLine("Формат: [grey]Код1:Количество1;Код2:Количество2;...[/]");
                    AnsiConsole.MarkupLine("Пример: [grey]A1004292:2;A1004293:0.5[/]");
                    
                    var input = AnsiConsole.Ask<string>("Введите заказ:");
                    
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        AnsiConsole.MarkupLine("[red]Ввод не может быть пустым[/]");
                        continue;
                    }
                    
                    var items = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    validInput = true;
                    
                    foreach (var item in items)
                    {
                        var parts = item.Split(':');
                        if (parts.Length != 2)
                        {
                            AnsiConsole.MarkupLine($"[red]Некорректный формат: {item}[/]");
                            validInput = false;
                            break;
                        }
                        
                        var code = parts[0].Trim();
                        var quantityStr = parts[1].Trim();
                        
                        if (!decimal.TryParse(quantityStr, out var quantity) || quantity <= 0)
                        {
                            AnsiConsole.MarkupLine($"[red]Некорректное количество: {quantityStr}[/]");
                            validInput = false;
                            break;
                        }
                        
                        var dish = dishes.FirstOrDefault(d => d.Article == code || d.Id == code);
                        if (dish == null)
                        {
                            AnsiConsole.MarkupLine($"[red]Блюдо с кодом '{code}' не найдено[/]");
                            validInput = false;
                            break;
                        }
                        
                        order.Add(new OrderItemDto
                        {
                            DishId = dish.Id,
                            DishName = dish.Name,
                            Quantity = quantity,
                            UnitPrice = dish.Price
                        });
                    }
                    
                    if (!validInput)
                    {
                        AnsiConsole.MarkupLine("[yellow]Повторите ввод...[/]\n");
                    }
                }
                
                AnsiConsole.MarkupLine("\n[green]5. Отправка заказа на сервер...[/]");
                var sendResult = await mediator.Send(new SendOrderCommand { Items = order });
                
                if (sendResult.Success)
                {
                    AnsiConsole.MarkupLine("[bold green]✓ УСПЕХ[/]");
                    AnsiConsole.MarkupLine($"[grey]ID заказа: {sendResult.Data}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine($"[bold red]✗ ОШИБКА: {sendResult.ErrorMessage}[/]");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error running application");
                AnsiConsole.MarkupLine($"[bold red]Критическая ошибка: {ex.Message}[/]");
            }
            
            AnsiConsole.MarkupLine("\n[yellow]Нажмите любую клавишу для выхода...[/]");
            Console.ReadKey();
        }
    }
}
