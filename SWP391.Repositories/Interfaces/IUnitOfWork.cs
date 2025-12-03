namespace SWP391.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        //PromotionsQuangNmRepository PromotionsQuangNmRepository { get; }
        //PromotionUsageQuangNmRepository PromotionUsageQuangNmRepository { get; }
        //SystemUserAccountRepository SystemUserAccountRepository { get; }


        int SaveChangesWithTransaction();
        Task<int> SaveChangesWithTransactionAsync();
    }
}
