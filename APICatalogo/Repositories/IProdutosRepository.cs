using APICatalogo.Models;

namespace APICatalogo.Repositories;

public interface IProdutosRepository : IRepository<Produto>
{
    IEnumerable<Produto> GetProdutosPorCategoria(int id);
}
