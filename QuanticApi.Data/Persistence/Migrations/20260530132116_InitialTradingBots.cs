using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanticApi.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialTradingBots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trading_bots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    exchange = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    strategy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    allocation = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trading_bots", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trading_bots");
        }
    }
}
