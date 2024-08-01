using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context)
    : Repository<Categoria>(context), ICategoriaRepository
{
    public PageList<Categoria> GetCategorias(CategoriasParameters parameters)
    {
        var categorias = GetAll()
            .OrderBy(p => p.CategoriaId).AsQueryable();

        var categoriasOrdenadas = PageList<Categoria>
            .ToPageDLis(categorias, parameters.PageNumber, parameters.PageSize);

        return categoriasOrdenadas;
    }

    PageList<Categoria> ICategoriaRepository.GetCategoriasFiltroNome(CategoriaFiltroNome filtroNome)
    {
        var categorias = GetAll().AsQueryable();

        if (!string.IsNullOrEmpty(filtroNome.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(filtroNome.Nome));
        }

        var categoriasFiltradas = PageList<Categoria>
            .ToPageDLis(categorias, filtroNome.PageNumber, filtroNome.PageSize);

        return categoriasFiltradas;
    }
}
