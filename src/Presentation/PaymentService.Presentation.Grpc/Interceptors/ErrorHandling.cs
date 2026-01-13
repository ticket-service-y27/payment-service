using Grpc.Core;
using Grpc.Core.Interceptors;
using PaymentService.Application.Models.Payments;
using PaymentService.Application.Models.Transactions;
using PaymentService.Application.Models.Wallets;

namespace PaymentService.Presentation.Grpc.Interceptors;

public class ErrorHandling : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (PaymentException e)
        {
            var error = new RpcException(
                new Status(StatusCode.InvalidArgument, e.Message));

            throw error;
        }
        catch (WalletException e)
        {
            var error = new RpcException(
                new Status(StatusCode.InvalidArgument, e.Message));

            throw error;
        }
        catch (WalletTransactionException e)
        {
            var error = new RpcException(
                new Status(StatusCode.InvalidArgument, e.Message));

            throw error;
        }
        catch (Exception e)
        {
            var error = new RpcException(
                new Status(StatusCode.Internal, e.Message));

            throw error;
        }
    }
}