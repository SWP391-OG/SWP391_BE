using SWP391.Repositories.Repositories;

namespace SWP391.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        UserRepository UserRepository { get; }
        VerificationCodeRepository VerificationCodeRepository { get; }
        int SaveChangesWithTransaction();
        Task<int> SaveChangesWithTransactionAsync();
    }
}
