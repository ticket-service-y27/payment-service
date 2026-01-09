using PaymentService.Application.Models.Wallets;

namespace PaymentService.Application.Contracts.Wallets;

public interface IWalletService
{
    Task<Wallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken);

    Task<Wallet?> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task CreateWalletAsync(long userId, CancellationToken cancellationToken);

    Task TopUpWalletAsync(long walletId, long amount, CancellationToken cancellationToken);

    Task SetBlockStatusAsync(long walletId, bool isBlocked, CancellationToken cancellationToken);
}