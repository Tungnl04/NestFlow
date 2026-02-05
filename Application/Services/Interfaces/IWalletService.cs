using NestFlow.Models;

namespace NestFlow.Application.Services.Interfaces
{
    public interface IWalletService
    {
        Task<Wallet> GetOrCreateWalletAsync(long landlordId);
        Task<bool> LockBalanceAsync(long walletId, decimal amount, string relatedType, long relatedId, string note);
        Task<bool> ReleaseLockedBalanceAsync(long walletId, decimal amount, string relatedType, long relatedId, string note);
        Task<bool> TransferLockedToAvailableAsync(long walletId, decimal amount, string relatedType, long relatedId, string note);
        Task<bool> RefundToRenterAsync(long renterId, decimal amount, string relatedType, long relatedId, string note);
        Task<decimal> GetAvailableBalanceAsync(long landlordId);
        Task<decimal> GetLockedBalanceAsync(long landlordId);
    }
}
