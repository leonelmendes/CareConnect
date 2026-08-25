using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTabelaTarefas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TituloAdHoc",
                table: "TaskLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimestampExecucao",
                table: "TaskLogs",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "TaskLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataHoraAgendada",
                table: "TaskLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Titulo",
                table: "TaskLogs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_UtenteId",
                table: "TaskLogs",
                column: "UtenteId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLogs_Patients_UtenteId",
                table: "TaskLogs",
                column: "UtenteId",
                principalTable: "Patients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskLogs_Patients_UtenteId",
                table: "TaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskLogs_UtenteId",
                table: "TaskLogs");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "TaskLogs");

            migrationBuilder.DropColumn(
                name: "DataHoraAgendada",
                table: "TaskLogs");

            migrationBuilder.DropColumn(
                name: "Titulo",
                table: "TaskLogs");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimestampExecucao",
                table: "TaskLogs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TituloAdHoc",
                table: "TaskLogs",
                type: "text",
                nullable: true);
        }
    }
}
