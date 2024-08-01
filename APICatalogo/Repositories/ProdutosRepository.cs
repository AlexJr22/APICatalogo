using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;

namespace APICatalogo.Repositories;

public class ProdutosRepository(AppDbContext context)
    : Repository<Produto>(context), IProdutosRepository
{
    //public IEnumerable<Produto> GetProdutos(ProdutosParameters parameters)
    //{
    //    return GetAll()
    //        .OrderBy(prod => prod.Nome)
    //        .Skip((parameters.PageNumber - 1) * parameters.PageSize)
    //        .Take(parameters.PageSize)
    //        .ToList();
    //}

    public PageList<Produto> GetProdutos(ProdutosParameters parameters)
    {
        var produtos = GetAll()
            .OrderBy(p => p.ProdutoId).AsQueryable();

        var produtosOrdenados = PageList<Produto>
            .ToPageDLis(produtos, parameters.PageNumber, parameters.PageSize);

        return produtosOrdenados;
    }

    public IEnumerable<Produto> GetProdutosPorCategoria(int id)
    {
        return GetAll().Where(c => c.CategoriaId == id);
    }

    public PageList<Produto> GetProdutoFiltroPreco(ProdutoFiltroPreco filtroPreco)
    {
        var produtos = GetAll().AsQueryable();

        if (filtroPreco.Preco.HasValue && !string.IsNullOrEmpty(filtroPreco.PrecoCriterio))
        {
            if (filtroPreco.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco > filtroPreco.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (filtroPreco.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco < filtroPreco.Preco.Value).OrderBy(p => p.Preco);
            }
            else if (filtroPreco.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
            {
                produtos = produtos.Where(p => p.Preco == filtroPreco.Preco.Value).OrderBy(p => p.Preco);
            }
        }
        var produtosFiltrados = PageList<Produto>.ToPageDLis(produtos, filtroPreco.PageNumber, filtroPreco.PageSize);
        return produtosFiltrados;
    }
}
