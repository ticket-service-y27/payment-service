using PaymentService.Application.Models.Wallets;

namespace PaymentService.Application.Abstractions.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken);

    Task<Wallet?> GetByUserIdAsync(long userId, CancellationToken cancellationToken);

    Task CreateAsync(long userId, CancellationToken cancellationToken);

    Task UpdateAsync(long walletId, long amount, CancellationToken cancellationToken);

    Task SetBlockedAsync(long walletId, bool isBlocked, CancellationToken cancellationToken);
}