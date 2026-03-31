using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ViaYou.Migrations
{
    /// <inheritdoc />
    public partial class AddMutualFunds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MutualFunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FundName = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    NAV = table.Column<decimal>(type: "TEXT", nullable: false),
                    Returns1Y = table.Column<decimal>(type: "TEXT", nullable: false),
                    Returns3Y = table.Column<decimal>(type: "TEXT", nullable: false),
                    Returns5Y = table.Column<decimal>(type: "TEXT", nullable: false),
                    ExpenseRatio = table.Column<decimal>(type: "TEXT", nullable: false),
                    MinimumInvestment = table.Column<decimal>(type: "TEXT", nullable: false),
                    RiskLevel = table.Column<string>(type: "TEXT", nullable: false),
                    IsDirectPlan = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutualFunds", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MutualFunds");
        }
    }
}
