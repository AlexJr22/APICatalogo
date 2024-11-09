using APICatalogo.Context;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APICatalogo_xUnitTest.UnitTests
{
    public class ProdutosUnitTestController
    {
        public IUnitOfWork repository;
        public IMapper mapper;
        public static DbContextOptions<AppDbContext> dbContextOptions { get; }

        public static string connectionString =
            "Data Source=C:\\Users\\alexj\\Documents\\Curso Macoratti\\AspNet\\Projetos\\APICatalogo\\APICatalogo\\mydb.db";

        // configurando o acesso ao dbcontext
        static ProdutosUnitTestController()
        {
            dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=C:\\Users\\alexj\\Documents\\Curso Macoratti\\AspNet\\Projetos\\APICatalogo\\APICatalogo\\mydb.db")
                .Options;
        }

        public ProdutosUnitTestController()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ProdutoDTOMappingProfile());
            });

            mapper = config.CreateMapper();
            var context = new AppDbContext(dbContextOptions);
            repository = new UnitOfWork(context);
        }
    }
}
