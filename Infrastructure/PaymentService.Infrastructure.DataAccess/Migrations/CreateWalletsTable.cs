using FluentMigrator;

namespace PaymentService.Infrastructure.DataAccess.Migrations;

[Migration(1, "create wallets table")]
public class CreateWalletsTable : Migration
{
    public override void Up()
    {
        Create.Table("wallets")
            .WithColumn("wallet_id").AsInt64().PrimaryKey().Identity()
            .WithColumn("user_id").AsInt64().NotNullable().Unique()
            .WithColumn("balance").AsInt64().NotNullable()
            .WithColumn("is_blocked").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentDateTimeOffset);
    }

    public override void Down()
    {
        Delete.Table("wallets");
    }
}