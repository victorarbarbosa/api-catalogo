using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APICatalogo.Migrations
{
    /// <inheritdoc />
    public partial class PopulaCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into Categorias (Name, ImageUrl) Values('Bebidas', 'bebidas.jpg')");
            migrationBuilder.Sql("insert into Categorias (Name, ImageUrl) Values('Lanches', 'lanches.jpg')");
            migrationBuilder.Sql("insert into Categorias (Name, ImageUrl) Values('Sobremesas', 'sobremesas.jpg')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from Categorias");
        }
    }
}
