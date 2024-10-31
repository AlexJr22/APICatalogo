using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ProdutosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProdutosController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet("produtosPorCategoria/{id:int}")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosPorCategoria(int id)
    {
        var produtos = await _unitOfWork.produtosRepository.GetProdutosPorCategoriaAsync(id);

        if (produtos is null)
        {
            return NotFound();
        }

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [Authorize]
    [HttpGet]
    [Authorize(Policy = "User")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get()
    {
        var produtos = await _unitOfWork.produtosRepository.GetAllAsync();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> Get([FromQuery] ProdutosParameters _params)
    {
        var produtos = await _unitOfWork.produtosRepository.GetProdutosAsync(_params);

        return ObterProdutos(produtos);
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(IPagedList<Produto>? produtos)
    {
        if (produtos is null)
            return NotFound();

        var metadata = new
        {
            produtos.Count,
            produtos.PageSize,
            produtos.PageCount,
            produtos.TotalItemCount,
            produtos.HasNextPage,
            produtos.HasPreviousPage,
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpGet("filter/preco/pagination")]
    public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetProdutosFilterPreco(
        [FromQuery] ProdutoFiltroPreco produtoFiltroPreco)
    {
        var produtos = await _unitOfWork.produtosRepository.GetProdutoFiltroPrecoAsync(produtoFiltroPreco);

        return ObterProdutos(produtos);
    }

    [HttpGet("{id:int}", Name = "ObterProduto")]
    public async Task<ActionResult<ProdutoDTO>> Get(int id)
    {
        var produto = await _unitOfWork.produtosRepository.GetAsync(
            prod => prod.ProdutoId == id);

        if (produto is null)
            return NotFound("Produto Não Encontrado");

        var produtoDto = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoDTO>> Post(ProdutoDTO produtoDto)
    {
        var produto = _mapper.Map<Produto>(produtoDto);

        var newProduto = _unitOfWork.produtosRepository.Create(produto);
        await _unitOfWork.CommitAsync();

        if (newProduto is null)
            return BadRequest("Erro ao criar o novo produto!");

        return new CreatedAtRouteResult
            ("ObterProduto",
            new { id = produtoDto.ProdutoId },
            produtoDto);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto);

        var novoProduto = _unitOfWork.produtosRepository.Update(produto);
        await _unitOfWork.CommitAsync();

        if (novoProduto is not null)
            return Ok(produtoDto);

        return StatusCode(500, "Falha ao Atualizar o produto");
    }

    [HttpPatch("updatePartial/{id:int}")]
    public async Task<ActionResult<ProdutoDTOUpdateResponce>> UpdatePartial(int id,
        JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
    {
        if (patchProdutoDTO is null || id <= 0)
            return BadRequest();

        var produto = await _unitOfWork.produtosRepository.GetAsync(
            prod => prod.ProdutoId == id);

        if (produto is null)
            return NotFound();

        var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);
        patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);
        
        if (!ModelState.IsValid || TryValidateModel(produtoUpdateRequest))
            return BadRequest();

        _mapper.Map(produtoUpdateRequest, produto);

        _unitOfWork.produtosRepository.Update(produto);
        await _unitOfWork.CommitAsync();

        return Ok(_mapper.Map<ProdutoDTOUpdateResponce>(produto));
    }


    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ProdutoDTO>> Delete(int id)
    {
        var produto = await _unitOfWork.produtosRepository.GetAsync(p => p.ProdutoId == id);

        if (produto is null)
            return BadRequest("Falha ao deletar o produto!");

        var produtoDeletado = _unitOfWork.produtosRepository.Delete(produto);
        await _unitOfWork.CommitAsync();

        var produtoDtoDeletado = _mapper.Map<ProdutoDTO>(produtoDeletado);

        if (produtoDeletado is not null)
        {
            return Ok(produtoDtoDeletado);
        }

        return StatusCode(500, "Falha ao deletar o Produto!");
    }
}
