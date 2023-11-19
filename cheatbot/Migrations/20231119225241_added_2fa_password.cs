using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cheatbot.Migrations
{
    /// <inheritdoc />
    public partial class added_2fa_password : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "_2fa_password",
                table: "Drops",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<long>(
                name: "tg_id",
                table: "Channels",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "_2fa_password",
                table: "Drops");

            migrationBuilder.AlterColumn<long>(
                name: "tg_id",
                table: "Channels",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
