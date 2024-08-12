namespace APICatalogo.Repositories;

public interface IUnitOfWork
{
    IProdutosRepository produtosRepository { get; }
    ICategoriaRepository categoriaRepository { get; }

    Task CommitAsync();
}
