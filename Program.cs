using Carpark.Models; 
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Carpark.Interface;
using System;
using Carpark;
using System.Diagnostics;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Запуск системи управління автопарком...");

      
        using (var context = new ApplicationDbContext())
        {
            
            using (var unitOfWork = new UnitOfWork(context))
            {
                var activeVehicles = await unitOfWork.Vehicles.GetActiveVehiclesAsync();
                Console.WriteLine("\nСписок активних автомобілів:");
                foreach (var v in activeVehicles)
                {
                    Console.WriteLine($"- ID: {v.Id}, Номер: {v.LicensePlate}, Статус: {v.Status}");
                }

                Console.WriteLine("\nСпроба оформити оренду...");
                var rentalService = new RentalService(unitOfWork);
                await rentalService.ProcessComplexRentalOperationAsync();
            }

            // === ІНІЦІАЛІЗАЦІЯ MONGODB ===
            
            using (var testContext = new ApplicationDbContext())
            {
                var mongoClient = new MongoClient("mongodb://localhost:27017");
                var damageReportsCollection = mongoClient
                    .GetDatabase("CarRentalDB")
                    .GetCollection<DamageReport>("DamageReports");

                await RunPerformanceTestAsync(testContext, damageReportsCollection);

                Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
                Console.ReadKey();
            }
        } 

        Console.WriteLine("\nПрограма завершила роботу. Натисніть будь-яку клавішу для виходу...");
        Console.ReadKey();
    }

    /// <summary>
    /// Метод для порівняльного стрес-тесту швидкодії між MS SQL (Entity Framework Core) та MongoDB.
    /// </summary>
    public static async Task RunPerformanceTestAsync(ApplicationDbContext dbContext, IMongoCollection<DamageReport> mongoCollection)
    {
        Console.WriteLine("\n=== СТАРТ ТЕСТУВАННЯ ШВИДКОДІЇ (10 000 запитів) ===");
        Console.WriteLine("Будь ласка, зачекайте кілька секунд...\n");

        int iterations = 10000;

        // 1. Тест MS SQL (Читання через JOIN трьох таблиць)
        var stopwatchSql = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var sqlReport = await dbContext.SqlDamageReports
                .AsNoTracking()
                .Include(r => r.Details)
                    .ThenInclude(d => d.Photos)
                .FirstOrDefaultAsync(r => r.RentalId == 2);
        }
        stopwatchSql.Stop();
        Console.WriteLine($"[MS SQL] Читання {iterations} разів (JOIN 3 таблиць): {stopwatchSql.ElapsedMilliseconds} мс");

        // 2. Тест MongoDB (Читання єдиного вкладеного документа)
        var stopwatchMongo = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var mongoReport = await mongoCollection
                .Find(r => r.RentalId == 2)
                .FirstOrDefaultAsync();
        }
        stopwatchMongo.Stop();
        Console.WriteLine($"[MongoDB] Читання {iterations} разів (один документ): {stopwatchMongo.ElapsedMilliseconds} мс");

        // 3. Аналітика та порівняння
        if (stopwatchSql.ElapsedMilliseconds > stopwatchMongo.ElapsedMilliseconds && stopwatchMongo.ElapsedMilliseconds > 0)
        {
            double difference = (double)stopwatchSql.ElapsedMilliseconds / stopwatchMongo.ElapsedMilliseconds;
            Console.WriteLine($"\nВисновок: MongoDB працює у {Math.Round(difference, 1)} разів швидше за MS SQL під навантаженням.");
        }
        else if (stopwatchMongo.ElapsedMilliseconds >= stopwatchSql.ElapsedMilliseconds)
        {
            Console.WriteLine("\nВисновок: Обидві бази відпрацювали успішно.");
        }

        Console.WriteLine("===================================================");
    }
}