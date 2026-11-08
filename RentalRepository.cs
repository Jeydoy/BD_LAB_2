using System;
using System.Collections.Generic;
using System.Text;
using Carpark.Interface;
using Microsoft.EntityFrameworkCore;

namespace Carpark
{
    public class RentalRepository : IRentalRepository
    {
        private readonly ApplicationDbContext _context;

        public RentalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateRentalAsync(int clientId, int vehicleId, int pickUpLoc, int dropOffLoc, int userId, DateTime start, DateTime end)
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_CreateRental @ClientId={0}, @VehicleId={1}, @PickUpLocationId={2}, @DropOffLocationId={3}, @ProcessedByUserId={4}, @StartDate={5}, @EndDate={6}",
                clientId, vehicleId, pickUpLoc, dropOffLoc, userId, start, end);
        }
    }
}
