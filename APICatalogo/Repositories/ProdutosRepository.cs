using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repositories;

public class ProdutosRepository(AppDbContext context) 
    : Repository<Produto>(context), IProdutosRepository
{
    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }
}
