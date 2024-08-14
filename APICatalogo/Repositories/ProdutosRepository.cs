using APICatalogo.Context;
using APICatalogo.Models;
using APICatalogo.Pagination;
using X.PagedList;

namespace APICatalogo.Repositories;

public class ProdutosRepository(AppDbContext context)
    : Repository<Produto>(context), IProdutosRepository
{
    public async Task<IPagedList<Produto>> GetProdutosAsync(ProdutosParameters parameters)
    {
        var produtos = await GetAllAsync();

        var produtosOrdenados = produtos
            .OrderBy(p => p.ProdutoId).AsQueryable();

        var resultado = await produtosOrdenados.ToPagedListAsync(
            parameters.PageNumber, parameters.PageSize);

        return resultado;
    }

    public async Task<IEnumerable<Produto>> GetProdutosPorCategoriaAsync(int id)
    {
        var produtos = await GetAllAsync();
        var produtosOrdenados = produtos.Where(c => c.CategoriaId == id);

        return produtosOrdenados;
    }

    public async Task<IPagedList<Produto>> GetProdutoFiltroPrecoAsync(ProdutoFiltroPreco filtroPreco)
    {
        var resultado = await GetAllAsync();

        var produtos = resultado.AsQueryable();

        if (filtroPreco.Preco.HasValue && !string.IsNullOrEmpty(filtroPreco.PrecoCriterio))
        {
            if (filtroPreco.PrecoCriterio.Equals("maior", StringComparison.OrdinalIgnoreCase))
                produtos = produtos.Where(p => p.Preco > filtroPreco.Preco.Value).OrderBy(p => p.Preco);

            if (filtroPreco.PrecoCriterio.Equals("menor", StringComparison.OrdinalIgnoreCase))
                produtos = produtos.Where(p => p.Preco < filtroPreco.Preco.Value).OrderBy(p => p.Preco);

            if (filtroPreco.PrecoCriterio.Equals("igual", StringComparison.OrdinalIgnoreCase))
                produtos = produtos.Where(p => p.Preco == filtroPreco.Preco.Value).OrderBy(p => p.Preco);
        }

        var produtosFiltrados = await produtos.ToPagedListAsync(filtroPreco.PageNumber, filtroPreco.PageSize);

        return produtosFiltrados;
    }
}
