using Microsoft.EntityFrameworkCore;
using NestFlow.Application.Services.Interfaces;
using NestFlow.Models;

namespace NestFlow.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly NestFlowSystemContext _context;
        private readonly ILogger<WalletService> _logger;

        public WalletService(NestFlowSystemContext context, ILogger<WalletService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Lấy hoặc tạo ví cho landlord
        /// </summary>
        public async Task<Wallet> GetOrCreateWalletAsync(long landlordId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.LandlordId == landlordId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    LandlordId = landlordId,
                    AvailableBalance = 0,
                    LockedBalance = 0,
                    Currency = "VND",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Wallets.Add(wallet);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created new wallet for landlord {landlordId}");
            }

            return wallet;
        }

        /// <summary>
        /// Khóa số dư (khi có booking mới chờ approve)
        /// </summary>
        public async Task<bool> LockBalanceAsync(long walletId, decimal amount, string relatedType, long relatedId, string note)
        {
            try
            {
                _logger.LogInformation($"=== START LockBalanceAsync ===");
                _logger.LogInformation($"WalletId: {walletId}, Amount: {amount}, Type: {relatedType}, Id: {relatedId}");
                
                var wallet = await _context.Wallets.FindAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogError($"Wallet {walletId} not found");
                    return false;
                }

                _logger.LogInformation($"Wallet found. Current LockedBalance: {wallet.LockedBalance}, AvailableBalance: {wallet.AvailableBalance}");

                // Tăng locked balance
                wallet.LockedBalance += amount;
                wallet.UpdatedAt = DateTime.Now;

                _logger.LogInformation($"Updating wallet. New LockedBalance: {wallet.LockedBalance}");

                // Tạo transaction log
                var transaction = new WalletTransaction
                {
                    WalletId = walletId,
                    Direction = "in",
                    Amount = amount,
                    RelatedType = relatedType,
                    RelatedId = relatedId,
                    Status = "locked",
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.WalletTransactions.Add(transaction);
                
                _logger.LogInformation("Saving changes to database...");
                await _context.SaveChangesAsync();

                _logger.LogInformation($"✅ Successfully locked {amount} VND in wallet {walletId} for {relatedType} {relatedId}");
                _logger.LogInformation("=== END LockBalanceAsync SUCCESS ===");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== ERROR LockBalanceAsync ===");
                _logger.LogError($"Exception: {ex.GetType().Name}");
                _logger.LogError($"Message: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"InnerException: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// Giải phóng số dư bị khóa (khi landlord từ chối booking)
        /// </summary>
        public async Task<bool> ReleaseLockedBalanceAsync(long walletId, decimal amount, string relatedType, long relatedId, string note)
        {
            try
            {
                var wallet = await _context.Wallets.FindAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogError($"Wallet {walletId} not found");
                    return false;
                }

                if (wallet.LockedBalance < amount)
                {
                    _logger.LogError($"Insufficient locked balance in wallet {walletId}");
                    return false;
                }

                // Giảm locked balance
                wallet.LockedBalance -= amount;
                wallet.UpdatedAt = DateTime.Now;

                // Tạo transaction log
                var transaction = new WalletTransaction
                {
                    WalletId = walletId,
                    Direction = "out",
                    Amount = amount,
                    RelatedType = relatedType,
                    RelatedId = relatedId,
                    Status = "released",
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.WalletTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Released {amount} VND from wallet {walletId} for {relatedType} {relatedId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error releasing balance: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Chuyển từ locked sang available (khi landlord chấp nhận booking)
        /// </summary>
        public async Task<bool> TransferLockedToAvailableAsync(long walletId, decimal amount, string relatedType, long relatedId, string note)
        {
            try
            {
                _logger.LogInformation($"=== START TransferLockedToAvailable ===");
                _logger.LogInformation($"WalletId: {walletId}, Amount: {amount}, Type: {relatedType}, Id: {relatedId}");
                
                var wallet = await _context.Wallets.FindAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogError($"Wallet {walletId} not found");
                    return false;
                }

                _logger.LogInformation($"Wallet found. Available: {wallet.AvailableBalance}, Locked: {wallet.LockedBalance}");

                if (wallet.LockedBalance < amount)
                {
                    _logger.LogError($"Insufficient locked balance! Need: {amount}, Have: {wallet.LockedBalance}");
                    return false;
                }

                _logger.LogInformation("Transferring from locked to available...");
                // Chuyển từ locked sang available
                wallet.LockedBalance -= amount;
                wallet.AvailableBalance += amount;
                wallet.UpdatedAt = DateTime.Now;

                // Tạo transaction log
                var transaction = new WalletTransaction
                {
                    WalletId = walletId,
                    Direction = "in",
                    Amount = amount,
                    RelatedType = relatedType,
                    RelatedId = relatedId,
                    Status = "completed",
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.WalletTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Transfer successful! New Available: {wallet.AvailableBalance}, New Locked: {wallet.LockedBalance}");
                _logger.LogInformation("=== END TransferLockedToAvailable SUCCESS ===");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"=== ERROR TransferLockedToAvailable ===");
                _logger.LogError($"Exception: {ex.Message}");
                _logger.LogError($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Hoàn tiền cho renter (khi landlord từ chối)
        /// </summary>
        public async Task<bool> RefundToRenterAsync(long renterId, decimal amount, string relatedType, long relatedId, string note)
        {
            try
            {
                // Tạo hoặc lấy ví của renter
                var renterWallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.LandlordId == renterId);

                if (renterWallet == null)
                {
                    renterWallet = new Wallet
                    {
                        LandlordId = renterId,
                        AvailableBalance = amount,
                        LockedBalance = 0,
                        Currency = "VND",
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.Wallets.Add(renterWallet);
                }
                else
                {
                    renterWallet.AvailableBalance += amount;
                    renterWallet.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Tạo transaction log
                var transaction = new WalletTransaction
                {
                    WalletId = renterWallet.WalletId,
                    Direction = "in",
                    Amount = amount,
                    RelatedType = relatedType,
                    RelatedId = relatedId,
                    Status = "refunded",
                    Note = note,
                    CreatedAt = DateTime.Now
                };

                _context.WalletTransactions.Add(transaction);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Refunded {amount} VND to renter {renterId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refunding to renter: {ex.Message}");
                return false;
            }
        }

        public async Task<decimal> GetAvailableBalanceAsync(long landlordId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.LandlordId == landlordId);
            return wallet?.AvailableBalance ?? 0;
        }

        public async Task<decimal> GetLockedBalanceAsync(long landlordId)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.LandlordId == landlordId);
            return wallet?.LockedBalance ?? 0;
        }
    }
}
