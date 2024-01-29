using APICatalogo.Validations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace APICatalogo.DTOs
{
    public class ProdutoDTO
    {
        public int ProdutoId { get; set; }
        public string? Name { get; set; }
        public string? Descricao { get; set; }
        public decimal Preco { get; set; }
        public string? ImageUrl { get; set; }
        public int CategoriaId { get; set; }
    }
}
