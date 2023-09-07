using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PublisherData.Migrations
{
    /// <inheritdoc />
    public partial class authoridchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Authors",
                newName: "AuthorId");

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "AuthorId", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 1, "Rhoda", "Lerman" },
                    { 2, "Ruth", "Ozeki" },
                    { 3, "Sofia", "Segovia" },
                    { 4, "Ursula K.", "LeGuin" },
                    { 5, "Hugh", "Howey" },
                    { 6, "Isabelle", "Allende" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "AuthorId", "BasePrice", "PublishDate", "Title" },
                values: new object[,]
                {
                    { 1, 1, 0m, new DateTime(1989, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "In God's Ear" },
                    { 2, 2, 0m, new DateTime(2013, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "A Tale For the Time Being" },
                    { 3, 3, 0m, new DateTime(1969, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "The Left Hand of Darkness" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "AuthorId",
                keyValue: 3);

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Authors",
                newName: "Id");
        }
    }
}
