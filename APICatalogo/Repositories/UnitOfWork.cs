using APICatalogo.Context;

namespace APICatalogo.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IProdutosRepository? _produtosRepository;
    public ICategoriaRepository? _categoriaRepository;
    private AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IProdutosRepository produtosRepository
    {
        get
        {
            return _produtosRepository = _produtosRepository ?? new ProdutosRepository(_context);
        }
    }

    public ICategoriaRepository categoriaRepository
    {
        get
        {
            return _categoriaRepository = _categoriaRepository ?? new CategoriaRepository(_context);
        }
    }

    public void Commit()
    {
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
