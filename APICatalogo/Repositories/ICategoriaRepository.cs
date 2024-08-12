using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<PageList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParems);
    Task<PageList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriaFiltroNome filtroNome);
}
