using System;
using System.Collections.Generic;
using System.Text;
using Carpark.Interface;
using Microsoft.EntityFrameworkCore.Storage;

namespace Carpark
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public IVehicleRepository Vehicles { get; private set; }
        public IRentalRepository Rentals { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Vehicles = new VehicleRepository(_context);
            Rentals = new RentalRepository(_context);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
