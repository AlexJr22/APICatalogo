using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories;

public interface IProdutosRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id);

    Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters parameters);

    Task<IPagedList<Produto>> GetProdutoFiltroPrecoAsync(ProdutoFiltroPreco filtroPreco);
}
