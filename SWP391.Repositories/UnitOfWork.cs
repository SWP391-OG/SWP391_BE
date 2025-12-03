using SWP391.Repositories.Interfaces;

namespace SWP391.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        //private readonly FA25_PRN232_SE1717_G1_ElectricVehicleManagementContext _context;
        //private readonly PromotionUsageQuangNmRepository? _promotionUsageQuangNmRepository;
        //private readonly PromotionsQuangNmRepository? _promotionsQuangNmRepository;
        //private readonly SystemUserAccountRepository? _systemUserAccountRepository;

        public UnitOfWork()
        {
            //_context ??= new FA25_PRN232_SE1717_G1_ElectricVehicleManagementContext();
        }

        //public PromotionsQuangNmRepository PromotionsQuangNmRepository
        //{
        //    get { return _promotionsQuangNmRepository ?? new PromotionsQuangNmRepository(_context); }
        //}

        //public PromotionUsageQuangNmRepository PromotionUsageQuangNmRepository
        //{
        //    get { return _promotionUsageQuangNmRepository ?? new PromotionUsageQuangNmRepository(_context); }
        //}

        //public SystemUserAccountRepository SystemUserAccountRepository
        //{
        //    get { return _systemUserAccountRepository ?? new SystemUserAccountRepository(_context); }
        //}

        public int SaveChangesWithTransaction()
        {
            int result = -1;

            //using (var dbContextTransaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        result = _context.SaveChanges();
            //        dbContextTransaction.Commit();
            //    }
            //    catch (Exception)
            //    {
            //        result = -1;
            //        dbContextTransaction.Rollback();
            //    }
            //}

            return result;
        }

        public async Task<int> SaveChangesWithTransactionAsync()
        {
            int result = -1;

            //using (var dbContextTransaction = _context.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        result = await _context.SaveChangesAsync();
            //        dbContextTransaction.Commit();
            //    }
            //    catch (Exception)
            //    {
            //        result = -1;
            //        dbContextTransaction.Rollback();
            //    }
            //}

            return result;
        }
    }
}
