using APICatalogo.Context;
using APICatalogo.Models;

namespace APICatalogo.Repositories;

public class CategoriaRepository(AppDbContext context)
    : Repository<Categoria>(context), ICategoriaRepository
{ }
