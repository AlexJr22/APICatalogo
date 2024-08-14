using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters categoriasParems);
    Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriaFiltroNome filtroNome);
}
