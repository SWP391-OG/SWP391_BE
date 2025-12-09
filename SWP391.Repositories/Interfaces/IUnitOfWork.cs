using SWP391.Repositories.Repositories;

namespace SWP391.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        NotificationRepository NotificationRepository { get; }
        UserRepository UserRepository { get; }
        LocationRepository LocationRepository { get; }
        VerificationCodeRepository VerificationCodeRepository { get; }
        RoleRepository RoleRepository { get; }
        CategoryRepository CategoryRepository { get; }
        TicketRepository TicketRepository { get; }
        DepartmentRepository DepartmentRepository { get; }
        int SaveChangesWithTransaction();
        Task<int> SaveChangesWithTransactionAsync();
    }
}
