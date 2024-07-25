﻿using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class ProdutosRepository(AppDbContext context) 
    : Repository<Produto>(context), IProdutosRepository
{
    public IEnumerable<Produto> GetProdutos(ProdutosParameters parameters)
    {
        return GetAll()
            .OrderBy(prod => prod.Nome)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToList();
    }

    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }
}
