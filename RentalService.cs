using System;
using System.Collections.Generic;
using System.Text;
using Carpark.Interface;

namespace Carpark
{
    public class RentalService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RentalService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessComplexRentalOperationAsync()
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {          
                await _unitOfWork.Rentals.CreateRentalAsync(
                    clientId: 1,
                    vehicleId: 5,
                    pickUpLoc: 1,
                    dropOffLoc: 1,
                    userId: 1,
                    start: DateTime.Now,
                    end: DateTime.Now.AddDays(3));

                await _unitOfWork.Vehicles.SoftDeleteAsync(vehicleId: 2, deletedByUserId: 1);

                await _unitOfWork.CommitAsync();
                Console.WriteLine("Транзакція пройшла успішно! Оренду створено, авто списано.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Помилка транзакції. Усі зміни відкочено: {ex.Message}");
            }
        }
    
    }
}
