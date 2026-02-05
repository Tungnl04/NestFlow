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
                var wallet = await _context.Wallets.FindAsync(walletId);
                if (wallet == null)
                {
                    _logger.LogError($"Wallet {walletId} not found");
                    return false;
                }

                // Tăng locked balance
                wallet.LockedBalance += amount;
                wallet.UpdatedAt = DateTime.Now;

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
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Locked {amount} VND in wallet {walletId} for {relatedType} {relatedId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error locking balance: {ex.Message}");
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

                _logger.LogInformation($"Transferred {amount} VND from locked to available in wallet {walletId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error transferring balance: {ex.Message}");
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
