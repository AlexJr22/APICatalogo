using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using X.PagedList;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context)
    : Repository<Categoria>(context), ICategoriaRepository
{
    public async Task<IPagedList<Categoria>> GetCategoriasAsync(CategoriasParameters parameters)
    {
        var categorias = await GetAllAsync();

        var categoriasOrdenadas = categorias
            .OrderBy(categoria => categoria.CategoriaId).AsQueryable();

        //var resultado = PageList<Categoria>
        //    .ToPageDLis(categoriasOrdenadas, parameters.PageNumber, parameters.PageSize);
        var resultado = await categorias.ToPagedListAsync(
            parameters.PageNumber, parameters.PageSize);

        return resultado;
    }

    public async Task<IPagedList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriaFiltroNome filtroNome)
    {
        var categorias = await GetAllAsync();

        if (!string.IsNullOrEmpty(filtroNome.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(filtroNome.Nome));
        }

        //var categoriasFiltradas = PageList<Categoria>
        //    .ToPageDLis(categorias.AsQueryable(), filtroNome.PageNumber, filtroNome.PageSize);

        var categoriasFiltradas = await categorias.ToPagedListAsync(
            filtroNome.PageNumber, filtroNome.PageSize);

        return categoriasFiltradas;
    }
}
