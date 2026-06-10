using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class CriarDominioCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alergias",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "Patients",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Contacto",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactoEmergencia",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Patients",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Notas",
                table: "Patients",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_CarePlanId",
                table: "TaskLogs",
                column: "CarePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskLogs_ExecutorId",
                table: "TaskLogs",
                column: "ExecutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_GestorId",
                table: "Patients",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_CarePlans_PatientId",
                table: "CarePlans",
                column: "PatientId");

            migrationBuilder.AddForeignKey(
                name: "FK_CarePlans_Patients_PatientId",
                table: "CarePlans",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Users_GestorId",
                table: "Patients",
                column: "GestorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLogs_CarePlans_CarePlanId",
                table: "TaskLogs",
                column: "CarePlanId",
                principalTable: "CarePlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaskLogs_Users_ExecutorId",
                table: "TaskLogs",
                column: "ExecutorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CarePlans_Patients_PatientId",
                table: "CarePlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Users_GestorId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLogs_CarePlans_CarePlanId",
                table: "TaskLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskLogs_Users_ExecutorId",
                table: "TaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskLogs_CarePlanId",
                table: "TaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_TaskLogs_ExecutorId",
                table: "TaskLogs");

            migrationBuilder.DropIndex(
                name: "IX_Patients_GestorId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_CarePlans_PatientId",
                table: "CarePlans");

            migrationBuilder.DropColumn(
                name: "Alergias",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Contacto",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ContactoEmergencia",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Notas",
                table: "Patients");
        }
    }
}
