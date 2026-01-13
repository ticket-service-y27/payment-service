using Grpc.Core;
using PaymentService.Application.Contracts.WalletTransactions;
using PaymentService.Grpc.WalletTransactions;
using PaymentService.Presentation.Grpc.Mapper;
using WalletTransaction = PaymentService.Application.Models.Transactions.WalletTransaction;

namespace PaymentService.Presentation.Grpc.Services;

public class WalletTransactionGrpcService : WalletTransactionsService.WalletTransactionsServiceBase
{
    private readonly IWalletTransactionService _transactionService;
    private readonly ModelMapper _modelMapper;

    public WalletTransactionGrpcService(IWalletTransactionService transactionService, ModelMapper modelMapper)
    {
        _transactionService = transactionService;
        _modelMapper = modelMapper;
    }

    public override async Task<GetTransactionByIdResponse> GetById(
        GetTransactionByIdRequest request,
        ServerCallContext context)
    {
        WalletTransaction? transaction =
            await _transactionService.GetByIdAsync(request.TransactionId, context.CancellationToken);

        if (transaction == null)
        {
            return new GetTransactionByIdResponse();
        }

        return new GetTransactionByIdResponse
        {
            Transaction = _modelMapper.Map(transaction),
        };
    }

    public override async Task<GetTransactionsByWalletIdResponse> GetByWalletId(
        GetTransactionsByWalletIdRequest request,
        ServerCallContext context)
    {
        IAsyncEnumerable<WalletTransaction> transactions =
            await _transactionService.GetByWalletIdAsync(request.WalletId, context.CancellationToken, null, null, null);

        var resp = new GetTransactionsByWalletIdResponse();

        await foreach (WalletTransaction transaction in transactions)
        {
            PaymentService.Grpc.WalletTransactions.WalletTransaction tx = _modelMapper.Map(transaction);

            resp.Transaction.Add(tx);
        }

        return resp;
    }
}