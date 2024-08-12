using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface IProdutosRepository : IRepository<Produto>
{
    Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id);

    Task<PageList<Produto>> GetProdutosAsync(ProdutosParameters parameters);

    Task<PageList<Produto>> GetProdutoFiltroPrecoAsync(ProdutoFiltroPreco filtroPreco);
}
