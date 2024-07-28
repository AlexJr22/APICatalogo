using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    PageList<Categoria> GetCategorias(CategoriasParameters categoriasParems);
}
