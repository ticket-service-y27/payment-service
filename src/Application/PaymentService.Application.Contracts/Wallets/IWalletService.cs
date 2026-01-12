namespace PaymentService.Application.Contracts.Wallets;

public interface IWalletService
{
    Task<long> CreateWalletAsync(long userId, CancellationToken cancellationToken);

    Task TopUpWalletAsync(long walletId, long amount, CancellationToken cancellationToken);

    Task SetBlockStatusAsync(long walletId, bool isBlocked, CancellationToken cancellationToken);
}