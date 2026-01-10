using Npgsql;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Models.Transactions;
using System.Runtime.CompilerServices;

namespace PaymentService.Infrastructure.DataAccess.Repositories;

public class WalletTransactionRepository : IWalletTransactionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public WalletTransactionRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<WalletTransaction?> GetByIdAsync(long transactionId, CancellationToken ct)
    {
        const string sql = """
                           select transaction_id, wallet_id, type, amount, payment_id, created_at
                           from wallet_transactions
                           where transaction_id = @TransactionId
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@TransactionId", transactionId));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return new WalletTransaction(
            Id: reader.GetInt64(0),
            WalletId: reader.GetInt64(1),
            Type: reader.GetFieldValue<TransactionType>(2),
            Amount: reader.GetInt64(3),
            PaymentId: reader.IsDBNull(4) ? null : reader.GetInt64(4),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async Task<long?> CreateAsync(
        long walletId,
        TransactionType type,
        long amount,
        CancellationToken ct,
        long? paymentId)
    {
        const string sql = """
                           insert into wallet_transactions (wallet_id, type, amount, payment_id, created_at)
                           values (@WalletId, @Type, @Amount, @PaymentId, @Now)
                           returning transaction_id, wallet_id, type, amount, payment_id, created_at
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Type",
            Value = type,
            DataTypeName = "transaction_type",
        });
        command.Parameters.Add(new NpgsqlParameter("@Amount", amount));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@PaymentId",
            Value = paymentId ?? (object)DBNull.Value,
            DataTypeName = "bigint",
        });
        command.Parameters.Add(new NpgsqlParameter("@Now", DateTimeOffset.UtcNow));

        return (long?)(await command.ExecuteScalarAsync(ct) ?? null);
    }

    public async IAsyncEnumerable<WalletTransaction> GetByWalletIdAsync(
        long walletId,
        [EnumeratorCancellation] CancellationToken ct,
        DateTimeOffset? from,
        DateTimeOffset? last,
        long? cursor)
    {
        const string sql = """
                           select transaction_id, wallet_id, type, amount, payment_id, created_at
                           from wallet_transactions
                           where wallet_id = @WalletId
                             and (@Cursor is null or transaction_id > @Cursor)
                             and (@From is null or created_at >= @From)
                             and (@To is null or created_at <= @To)
                           order by transaction_id
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@Cursor",
            Value = cursor ?? (object)DBNull.Value,
            DataTypeName = "bigint",
        });
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@From",
            Value = from ?? (object)DBNull.Value,
            DataTypeName = "timestamptz",
        });
        command.Parameters.Add(new NpgsqlParameter
        {
            ParameterName = "@To",
            Value = last ?? (object)DBNull.Value,
            DataTypeName = "timestamptz",
        });

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            yield return new WalletTransaction(
                Id: reader.GetInt64(0),
                WalletId: reader.GetInt64(1),
                Type: reader.GetFieldValue<TransactionType>(2),
                Amount: reader.GetInt64(3),
                PaymentId: reader.IsDBNull(4) ? null : reader.GetInt64(4),
                CreatedAt: reader.GetFieldValue<DateTimeOffset>(5));
        }
    }
}