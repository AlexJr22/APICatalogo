using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace APICatalogo.Controllers;

[Route("[controller]")]
[ApiController]
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
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosPorCategoria(int id)
    {
        var produtos = _unitOfWork.produtosRepository.GetProdutosPorCategoria(id);

        if (produtos is null)
        {
            return NotFound();
        }

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProdutoDTO>> Get()
    {
        var produtos = _unitOfWork.produtosRepository.GetAll();

        var produtosDto = _mapper.Map<IEnumerable<ProdutoDTO>>(produtos);

        return Ok(produtosDto);
    }

    [HttpGet("pagination")]
    public ActionResult<IEnumerable<ProdutoDTO>> Get([FromQuery] ProdutosParameters _params)
    {
        var produtos = _unitOfWork.produtosRepository.GetProdutos(_params);

        return ObterProdutos(produtos);
    }

    private ActionResult<IEnumerable<ProdutoDTO>> ObterProdutos(PageList<Produto>? produtos)
    {
        if (produtos is null)
            return NotFound();

        var metadata = new
        {
            produtos.TotalCount,
            produtos.PageSize,
            produtos.CurrentPage,
            produtos.TotalPages,
            produtos.HasNext,
            produtos.HasPrivious
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(_mapper.Map<IEnumerable<ProdutoDTO>>(produtos));
    }

    [HttpGet("filter/preco/pagination")]
    public ActionResult<IEnumerable<ProdutoDTO>> GetProdutosFilterPreco(
        [FromQuery] ProdutoFiltroPreco produtoFiltroPreco)
    {
        var produtos = _unitOfWork.produtosRepository.GetProdutoFiltroPreco(produtoFiltroPreco);

        return ObterProdutos(produtos);
    }

    [HttpGet("{id:int}", Name = "ObterProduto")]
    public ActionResult<ProdutoDTO> Get(int id)
    {
        var produto = _unitOfWork.produtosRepository.Get(
            prod => prod.ProdutoId == id);

        if (produto is null)
            return NotFound("Produto Não Encontrado");

        var produtoDto = _mapper.Map<ProdutoDTO>(produto);

        return Ok(produtoDto);
    }

    [HttpPost]
    public ActionResult<ProdutoDTO> Post(ProdutoDTO produtoDto)
    {
        var produto = _mapper.Map<Produto>(produtoDto);

        var newProduto = _unitOfWork.produtosRepository.Create(produto);
        _unitOfWork.Commit();

        if (newProduto is null)
            return BadRequest("Erro ao criar o novo produto!");

        return new CreatedAtRouteResult
            ("ObterProduto",
            new { id = produtoDto.ProdutoId },
            produtoDto);
    }

    [HttpPut("{id:int}")]
    public ActionResult<ProdutoDTO> Put(int id, ProdutoDTO produtoDto)
    {
        if (id != produtoDto.ProdutoId)
            return BadRequest();

        var produto = _mapper.Map<Produto>(produtoDto);

        var novoProduto = _unitOfWork.produtosRepository.Update(produto);
        _unitOfWork.Commit();

        if (novoProduto is not null)
            return Ok(produtoDto);

        return StatusCode(500, "Falha ao Atualizar o produto");
    }

    [HttpPatch("updatePartial/{id:int}")]
    public ActionResult<ProdutoDTOUpdateResponce> UpdatePartial(int id,
        JsonPatchDocument<ProdutoDTOUpdateRequest> patchProdutoDTO)
    {
        if (patchProdutoDTO is null || id <= 0)
            return BadRequest();

        var produto = _unitOfWork.produtosRepository.Get(
            prod => prod.ProdutoId == id);

        if (produto is null)
            return NotFound();

        var produtoUpdateRequest = _mapper.Map<ProdutoDTOUpdateRequest>(produto);
        patchProdutoDTO.ApplyTo(produtoUpdateRequest, ModelState);

        if (!ModelState.IsValid || TryValidateModel(produtoUpdateRequest))
            return BadRequest();

        _mapper.Map(produtoUpdateRequest, produto);

        _unitOfWork.produtosRepository.Update(produto);
        _unitOfWork.Commit();

        return Ok(_mapper.Map<ProdutoDTOUpdateResponce>(produto));
    }


    [HttpDelete("{id:int}")]
    public ActionResult<ProdutoDTO> Delete(int id)
    {
        var produto = _unitOfWork.produtosRepository.Get(p => p.ProdutoId == id);

        if (produto is null)
            return BadRequest("Falha ao deletar o produto!");

        var produtoDeletado = _unitOfWork.produtosRepository.Delete(produto);
        _unitOfWork.Commit();

        var produtoDtoDeletado = _mapper.Map<ProdutoDTO>(produtoDeletado);

        if (produtoDeletado is not null)
        {
            return Ok(produtoDtoDeletado);
        }

        return StatusCode(500, "Falha ao deletar o Produto!");
    }
}