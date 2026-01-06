using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DevResourceAPI.Migrations
{
    /// <inheritdoc />
    public partial class ResourceSeedFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Resources",
                columns: new[] { "Id", "CategoryId", "Description", "Title", "Url" },
                values: new object[,]
                {
                    { 1, 1, "Kapsamlı .NET ve C# rehberi.", "Microsoft .NET Documentation", "https://learn.microsoft.com/dotnet/" },
                    { 2, 2, "Modern Frontend geliştirme kılavuzu.", "React Official Docs", "https://react.dev/" },
                    { 3, 3, "Web uygulama güvenliği için en kritik 10 risk listesi.", "OWASP Top Ten", "https://owasp.org/www-project-top-ten/" },
                    { 4, 4, "İleri seviye SQL ve DB yönetimi dersleri.", "PostgreSQL Tutorial", "https://www.postgresqltutorial.com/" },
                    { 5, 5, "Yapay zeka modelleri için açık kaynaklı kütüphane.", "TensorFlow Hub", "https://www.tensorflow.org/" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Resources",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
