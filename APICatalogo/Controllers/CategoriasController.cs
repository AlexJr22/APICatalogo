using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ServiceFilter(typeof(ApiLoggingFilter))]
[ApiController]
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(IRepository<Categoria> repository,
                                ILogger<CategoriasController> logger,
                                IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public ActionResult<IEnumerable<CategoriaDTO>> Get()
    {
        var categorias = _unitOfWork.categoriaRepository.GetAll();

        if (categorias is null)
            return NotFound();

        var categoriasDto = categorias.ListCategoriaDTO();
        
        return Ok(categoriasDto);
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public ActionResult<CategoriaDTO> Get(int id)
    {
        var categoria = _unitOfWork.categoriaRepository.Get(
            cat => cat.CategoriaId == id);

        if (categoria is null)
        {
            _logger.LogWarning("=======================================");
            _logger.LogWarning($"Categoria com id={id} não encontrada...");
            _logger.LogWarning("=======================================");
            return NotFound($"Categoria com id={id} não encontrada...");
        }

        var categoriaDto = categoria.ToCategoriaDTO();

        return Ok(categoriaDto);
    }

    [HttpPost]
    public ActionResult<CategoriaDTO> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning("Dados Inválidos!");
            return BadRequest("Dados inválidos");
        }

        var categoria = categoriaDto.ToCategoria();

        var newCategoria = _unitOfWork.categoriaRepository.Create(categoria);
        _unitOfWork.Commit();

        var newCategoriaDto = categoria.ToCategoriaDTO();

        return new CreatedAtRouteResult(
            "ObterCategoria",
            new { id = newCategoriaDto.CategoriaId },
            newCategoria);
    }

    [HttpPut("{id:int}")]
    public ActionResult<CategoriaDTO> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
            return BadRequest("Dados inválidos");

        var categoria = categoriaDto.ToCategoria();

        _unitOfWork.categoriaRepository.Update(categoria);
        _unitOfWork.Commit();

        return Ok(categoriaDto);
    }

    [HttpDelete("{id:int}")]
    public ActionResult<CategoriaDTO> Delete(int id)
    {
        var categoria = _unitOfWork.categoriaRepository.Get(c => c.CategoriaId == id);

        if (categoria is null)
        {
            return BadRequest("Não foi possível deletar a categoria!");
        }

        var categoriaExcluida = _unitOfWork.categoriaRepository.Delete(categoria);

        var CategoriaDto = categoriaExcluida.ToCategoriaDTO();

        return Ok(CategoriaDto);
    }
}
