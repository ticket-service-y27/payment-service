using Npgsql;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Models.Payments;
using System.Runtime.CompilerServices;

namespace PaymentService.Infrastructure.DataAccess.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PaymentRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Payment?> GetAsync(long paymentId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select payment_id, wallet_id, status, amount, created_at, updated_at, payload
                           from payments
                           where payment_id = @PaymentId
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@PaymentId", paymentId));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Payment(
            Id: reader.GetInt64(0),
            WalletId: reader.GetInt64(1),
            Status: reader.GetFieldValue<PaymentStatus>(2),
            Amount: reader.GetInt64(3),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
            UpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async Task<long?> CreateAsync(
        long walletId,
        long amount,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into payments (wallet_id, status, amount, created_at, updated_at)
                           values (@WalletId, @Status, @Amount, @Now, @Now)
                           returning payment_id
                           """;

        await using NpgsqlConnection connection =
            await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Status",
            Value = PaymentStatus.Pending,
            DataTypeName = "payment_status",
        });
        command.Parameters.Add(new NpgsqlParameter("@Amount", amount));
        command.Parameters.Add(new NpgsqlParameter("@Now", DateTimeOffset.UtcNow));

        return (long?)(await command.ExecuteScalarAsync(cancellationToken) ?? null);
    }

    public async Task UpdatePaymentAsync(long paymentId, PaymentStatus status, CancellationToken cancellationToken)
    {
        const string sql = """
                           update payments
                           set status = @Status,
                               updated_at = @Now
                           where payment_id = @PaymentId
                           """;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@PaymentId", paymentId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Status",
            Value = status,
            DataTypeName = "payment_status",
        });
        command.Parameters.Add(new NpgsqlParameter("@Now", now));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Payment> GetByWalletIdAsync(
        long walletId,
        [EnumeratorCancellation] CancellationToken cancellationToken,
        long? cursor)
    {
        const string sql = """
                           select payment_id, wallet_id, status, amount, created_at, updated_at
                           from payments
                           where wallet_id = @WalletId
                             and (@Cursor is null or payment_id > @Cursor)
                           order by payment_id
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Cursor",
            Value = cursor ?? (object)DBNull.Value,
            DataTypeName = "bigint",
        });

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            yield return new Payment(
                Id: reader.GetInt64(0),
                WalletId: reader.GetInt64(1),
                Status: reader.GetFieldValue<PaymentStatus>(2),
                Amount: reader.GetInt64(3),
                CreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
                UpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
        }
    }
}