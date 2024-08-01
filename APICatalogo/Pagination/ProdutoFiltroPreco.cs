namespace APICatalogo.Pagination
{
    // essa class é usada como parametros nos repositorios de produtos
    public class ProdutoFiltroPreco : QueryStringParameters
    {
        public decimal? Preco {  get; set; }
        public string? PrecoCriterio { get; set; }
    }
}
