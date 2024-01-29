using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repository
{
    public interface IProdutoRepository : IRepository<Produto>
    {
        PagedList<Produto> GetProdutos(PagingParameters produtosParameters);
        IEnumerable<Produto> GetProdutosPorPreco();
    }
}
