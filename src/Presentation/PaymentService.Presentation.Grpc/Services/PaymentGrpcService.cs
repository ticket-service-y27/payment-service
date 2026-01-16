using Grpc.Core;
using PaymentService.Application.Contracts.Payments;
using PaymentService.Grpc.Payments;
using PaymentService.Presentation.Grpc.Mapper;
using Payment = PaymentService.Application.Models.Payments.Payment;

namespace PaymentService.Presentation.Grpc.Services;

public class PaymentGrpcService : PaymentsService.PaymentsServiceBase
{
    private readonly IPaymentService _paymentService;
    private readonly ModelMapper _modelMapper;

    public PaymentGrpcService(IPaymentService paymentService, ModelMapper modelMapper)
    {
        _paymentService = paymentService;
        _modelMapper = modelMapper;
    }

    public override async Task<CreatePaymentResponse> CreatePayment(
        CreatePaymentRequest request,
        ServerCallContext context)
    {
        long paymentId =
            await _paymentService.CreatePaymentAsync(request.UserId, request.Amount, context.CancellationToken);

        return new CreatePaymentResponse
        {
            PaymentId = paymentId,
        };
    }

    public override async Task<GetPaymentByIdResponse> GetPaymentById(
        GetPaymentByIdRequest request,
        ServerCallContext context)
    {
        Payment? payment = await _paymentService.GetByIdAsync(request.PaymentId, context.CancellationToken);

        if (payment == null)
        {
            return new GetPaymentByIdResponse();
        }

        return new GetPaymentByIdResponse
        {
            Payment = _modelMapper.Map(payment),
        };
    }

    public override async Task<GetPaymentsResponse> GetPayments(GetPaymentsRequest request, ServerCallContext context)
    {
        IAsyncEnumerable<Payment> payments =
            await _paymentService.GetPaymentsAsync(request.WalletId, context.CancellationToken, request.Cursor);

        var resp = new GetPaymentsResponse();

        await foreach (Payment payment in payments)
        {
            PaymentService.Grpc.Payments.Payment pay = _modelMapper.Map(payment);

            resp.Payment.Add(pay);
        }

        return resp;
    }

    public override async Task<TransferPaymentToRefundedResponse> TransferPaymentToRefunded(
        TransferPaymentToRefundedRequest request,
        ServerCallContext context)
    {
        try
        {
            await _paymentService.TransferPaymentStatusToFailedAsync(
                request.PaymentId,
                context.CancellationToken);

            return new TransferPaymentToRefundedResponse
            {
                IsSuccess = true,
            };
        }
        catch
        {
            return new TransferPaymentToRefundedResponse
            {
                IsSuccess = false,
            };
        }
    }
}