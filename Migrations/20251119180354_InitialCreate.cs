using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Work360.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Work360_Events",
                columns: table => new
                {
                    EventID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    EventType = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Duration = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work360_Events", x => x.EventID);
                });

            migrationBuilder.CreateTable(
                name: "Work360_Meetings",
                columns: table => new
                {
                    MeetingID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    MinutesDuration = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work360_Meetings", x => x.MeetingID);
                });

            migrationBuilder.CreateTable(
                name: "Work360_Tasks",
                columns: table => new
                {
                    TaskID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    UserID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Priority = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    EstimateMinutes = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Description = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    TaskSituation = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    CreatedTask = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    FinalDateTask = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SpentMinutes = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work360_Tasks", x => x.TaskID);
                });

            migrationBuilder.CreateTable(
                name: "Work360_User",
                columns: table => new
                {
                    UserID = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Work360_User", x => x.UserID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Work360_Events");

            migrationBuilder.DropTable(
                name: "Work360_Meetings");

            migrationBuilder.DropTable(
                name: "Work360_Tasks");

            migrationBuilder.DropTable(
                name: "Work360_User");
        }
    }
}
