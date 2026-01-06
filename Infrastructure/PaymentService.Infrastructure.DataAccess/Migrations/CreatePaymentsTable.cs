using FluentMigrator;

namespace PaymentService.Infrastructure.DataAccess.Migrations;

[Migration(2, "create payments table")]
public class CreatePaymentsTable : Migration
{
    public override void Up()
    {
        Execute.Sql("CREATE TYPE IF NOT EXISTS payment_status AS ENUM('pending', 'succeeded', 'failed', 'refunded');");

        Create.Table("payments")
            .WithColumn("payment_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("wallet_id").AsInt64().NotNullable().ForeignKey("wallets", "wallet_id")
            .WithColumn("status").AsCustom("payment_status").NotNullable()
            .WithColumn("amount").AsInt64().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("payload").AsCustom("jsonb").NotNullable();
    }

    public override void Down()
    {
        Delete.Table("payments");
        Execute.Sql("DROP TYPE IF EXISTS payment_status;");
    }
}