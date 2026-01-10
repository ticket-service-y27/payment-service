using Npgsql;
using PaymentService.Application.Abstractions.Repositories;
using PaymentService.Application.Models.Wallets;

namespace PaymentService.Infrastructure.DataAccess.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public WalletRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Wallet?> GetByIdAsync(long walletId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select wallet_id, user_id, balance, is_blocked, created_at, updated_at
                           from wallets
                           where wallet_id = @WalletId
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Wallet(
            Id: reader.GetInt64(0),
            UserId: reader.GetInt64(1),
            Balance: reader.GetInt64(2),
            IsBlocked: reader.GetBoolean(3),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
            UpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async Task<Wallet?> GetByUserIdAsync(long userId, CancellationToken cancellationToken)
    {
        const string sql = """
                           select wallet_id, user_id, balance, is_blocked, created_at, updated_at
                           from wallets
                           where user_id = @UserId
                           """;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@UserId", userId));

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new Wallet(
            Id: reader.GetInt64(0),
            UserId: reader.GetInt64(1),
            Balance: reader.GetInt64(2),
            IsBlocked: reader.GetBoolean(3),
            CreatedAt: reader.GetFieldValue<DateTimeOffset>(4),
            UpdatedAt: reader.GetFieldValue<DateTimeOffset>(5));
    }

    public async Task<long?> CreateAsync(long userId, CancellationToken cancellationToken)
    {
        const string sql = """
                           insert into wallets (user_id, balance, is_blocked, created_at, updated_at)
                           values (@UserId, 0, false, @Now, @Now)
                           """;

        DateTimeOffset time = DateTimeOffset.UtcNow;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@UserId", userId));
        command.Parameters.Add(new NpgsqlParameter("@Now", time));

        return (long?)(await command.ExecuteScalarAsync(cancellationToken) ?? null);
    }

    public async Task UpdateAsync(long walletId, long newBalance, CancellationToken cancellationToken)
    {
        const string sql = """
                           update wallets
                           set balance = @Balance,
                               updated_at = @Now
                           where wallet_id = @WalletId
                           """;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter("@Balance", newBalance));
        command.Parameters.Add(new NpgsqlParameter("@Now", now));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SetBlockedAsync(long walletId, bool isBlocked, CancellationToken cancellationToken)
    {
        const string sql = """
                           update wallets
                           set is_blocked = @IsBlocked,
                               updated_at = @Now
                           where wallet_id = @WalletId
                           """;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add(new NpgsqlParameter("@WalletId", walletId));
        command.Parameters.Add(new NpgsqlParameter("@IsBlocked", isBlocked));
        command.Parameters.Add(new NpgsqlParameter("@Now", now));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}