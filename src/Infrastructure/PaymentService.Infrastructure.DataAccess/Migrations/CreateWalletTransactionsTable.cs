using FluentMigrator;

namespace PaymentService.Infrastructure.DataAccess.Migrations;

[Migration(3, "create wallet transaction table")]
public class CreateWalletTransactionsTable : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE transaction_type AS ENUM ('topup', 'payment', 'refund');");

        Create.Table("wallet_transactions")
            .WithColumn("transaction_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("wallet_id").AsInt64().NotNullable().ForeignKey("wallets", "wallet_id")
            .WithColumn("type").AsCustom("transaction_type").NotNullable()
            .WithColumn("amount").AsInt64().NotNullable()
            .WithColumn("payment_id").AsInt64().Nullable().ForeignKey("payments", "payment_id")
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);
    }

    public override void Down()
    {
        Delete.Table("wallet_transactions");
        Execute.Sql("DROP TYPE IF EXISTS transaction_type;");
    }
}