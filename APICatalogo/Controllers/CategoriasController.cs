using APICatalogo.DTOs;
using APICatalogo.DTOs.Mappings;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        var categorias = await _unitOfWork.categoriaRepository.GetAllAsync();

        if (categorias is null)
            return NotFound();

        var categoriasDto = categorias.ListCategoriaDTO();
        
        return Ok(categoriasDto);
    }

    [HttpGet("{id:int}", Name = "ObterCategoria")]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var categoria = await _unitOfWork.categoriaRepository.GetAsync(
            categ => categ.CategoriaId == id);

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

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery]CategoriasParameters _params)
    {
        var categorias = await _unitOfWork.categoriaRepository.GetCategoriasAsync(_params);

        return ObterCategorias(categorias);
    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(PageList<Categoria>? categorias)
    {
        if (categorias is null)
            return BadRequest();

        var metadata = new
        {
            categorias.TotalCount,
            categorias.PageSize,
            categorias.CurrentPage,
            categorias.TotalPages,
            categorias.HasNext,
            categorias.HasPrivious
        };

        Response.Headers.Append("X-pagination", JsonConvert.SerializeObject(metadata));

        var categotiasDto = categorias.ListCategoriaDTO();

        return Ok(categotiasDto);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery] CategoriaFiltroNome filtroNome)
    {
        var categorias = await _unitOfWork.categoriaRepository.GetCategoriasFiltroNomeAsync(filtroNome);

        return ObterCategorias(categorias);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning("Dados Inválidos!");
            return BadRequest("Dados inválidos");
        }

        var categoria = categoriaDto.ToCategoria();

        var newCategoria = _unitOfWork.categoriaRepository.Create(categoria);
        await _unitOfWork.CommitAsync();

        var newCategoriaDto = categoria.ToCategoriaDTO();

        return new CreatedAtRouteResult(
            "ObterCategoria",
            new { id = newCategoriaDto.CategoriaId },
            newCategoria);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id != categoriaDto.CategoriaId)
            return BadRequest("Dados inválidos");

        var categoria = categoriaDto.ToCategoria();

        _unitOfWork.categoriaRepository.Update(categoria);
        await _unitOfWork.CommitAsync();

        return Ok(categoriaDto);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _unitOfWork.categoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria is null)
        {
            return BadRequest("Não foi possível deletar a categoria!");
        }

        var categoriaExcluida = _unitOfWork.categoriaRepository.Delete(categoria);

        var CategoriaDto = categoriaExcluida.ToCategoriaDTO();

        return Ok(CategoriaDto);
    }
}