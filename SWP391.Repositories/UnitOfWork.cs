using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Interfaces;
using SWP391.Repositories.Repositories;

namespace SWP391.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FPTechnicalContext _context;
        private UserRepository _userRepository;
        private LocationRepository _locationRepository;
        private VerificationCodeRepository _verificationCodeRepository;

        public UnitOfWork(FPTechnicalContext context)
        {
            _context = context;
        }

        public UserRepository UserRepository    { get => _userRepository ??= new UserRepository(_context); }
        
        public VerificationCodeRepository VerificationCodeRepository { get => _verificationCodeRepository ??= new VerificationCodeRepository(_context); }
   
        public LocationRepository LocationRepository { get => _locationRepository ??= new LocationRepository(_context); }

        public int SaveChangesWithTransaction()
        {
            int result = -1;

            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    result = _context.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }

        public async Task<int> SaveChangesWithTransactionAsync()
        {
            int result = -1;

            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    result = await _context.SaveChangesAsync();
                    dbContextTransaction.Commit();
                }
                catch (Exception)
                {
                    result = -1;
                    dbContextTransaction.Rollback();
                }
            }

            return result;
        }
    }
}
