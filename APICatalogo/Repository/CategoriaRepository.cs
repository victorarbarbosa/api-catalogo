using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo.Repository
{
    public class CategoriaRepository : Repository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public IEnumerable<Categoria> GetCategoriaProdutos()
        {
            return Get().Include(c => c.Produtos);
        }

        public PagedList<Categoria> GetCategorias(PagingParameters produtosParameters)
        {
            return PagedList<Categoria>.ToPagedList(Get().OrderBy(c => c.CategoriaId), produtosParameters.PageNumber, produtosParameters.PageSize);
        }
    }
}
