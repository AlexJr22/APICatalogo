namespace APICatalogo.Pagination
{

    // essa classe tem as definições basicas da paginação
    // ela é usada em todas as classes que serviram os metódos dos repositorios
    public class QueryStringParameters
    {
        const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = MaxPageSize;
        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }
    }
}
