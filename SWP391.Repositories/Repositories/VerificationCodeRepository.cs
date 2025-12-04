using Microsoft.EntityFrameworkCore;
using SWP391.Repositories.Basic;
using SWP391.Repositories.DBContext;
using SWP391.Repositories.Models;

namespace SWP391.Repositories.Repositories
{
    public class VerificationCodeRepository : GenericRepository<VerificationCode>
    {
        public VerificationCodeRepository() => _context ??= new FPTechnicalContext();

        public VerificationCodeRepository(FPTechnicalContext context) => _context = context;

        public async Task<VerificationCode?> GetValidCodeAsync(string email, string code, string type)
        {
            return await _context.Set<VerificationCode>()
                .FirstOrDefaultAsync(vc =>
                    vc.Email == email &&
                    vc.Code == code &&
                    vc.Type == type &&
                    vc.IsUsed != true &&
                    vc.ExpiresAt > DateTime.UtcNow);
        }

        public async Task InvalidateAllCodesAsync(string email, string type)
        {
            var codes = await _context.Set<VerificationCode>()
                .Where(vc => vc.Email == email && vc.Type == type && vc.IsUsed != true)
                .ToListAsync();

            foreach (var code in codes)
            {
                code.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}