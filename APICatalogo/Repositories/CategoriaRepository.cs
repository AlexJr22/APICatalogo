﻿using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context)
    : Repository<Categoria>(context), ICategoriaRepository
{
    public async Task<PageList<Categoria>> GetCategoriasAsync(CategoriasParameters parameters)
    {
        var categorias = await GetAllAsync();

        var categoriasOrdenadas = categorias
            .OrderBy(categoria => categoria.CategoriaId).AsQueryable();

        var resultado = PageList<Categoria>
            .ToPageDLis(categoriasOrdenadas, parameters.PageNumber, parameters.PageSize);

        return resultado;
    }

    public async Task<PageList<Categoria>> GetCategoriasFiltroNomeAsync(CategoriaFiltroNome filtroNome)
    {
        var categorias = await GetAllAsync();

        if (!string.IsNullOrEmpty(filtroNome.Nome))
        {
            categorias = categorias.Where(c => c.Nome.Contains(filtroNome.Nome));
        }

        var categoriasFiltradas = PageList<Categoria>
            .ToPageDLis(categorias.AsQueryable(), filtroNome.PageNumber, filtroNome.PageSize);

        return categoriasFiltradas;
    }
}
