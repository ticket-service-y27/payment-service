using Grpc.Core;
using PaymentService.Application.Contracts.Wallets;
using PaymentService.Grpc.Wallets;

namespace PaymentService.Presentation.Grpc.Services;

public class WalletGrpcService : WalletService.WalletServiceBase
{
    private readonly IWalletService _walletService;

    public WalletGrpcService(IWalletService walletService)
    {
        _walletService = walletService;
    }

    public override async Task<TopUpWalletResponse> TopUpWallet(TopUpWalletRequest request, ServerCallContext context)
    {
        await _walletService.TopUpWalletAsync(request.WalletId, request.Amount, context.CancellationToken);

        return new TopUpWalletResponse();
    }

    public override async Task<SetBlockStatusResponse> SetBlockStatus(SetBlockStatusRequest request, ServerCallContext context)
    {
        await _walletService.SetBlockStatusAsync(request.WalletId, request.IsBlocked, context.CancellationToken);

        return new SetBlockStatusResponse();
    }
}