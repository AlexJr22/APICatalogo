using APICatalogo.Models;

namespace APICatalogo.DTOs.Mappings;

public static class CategoriaDtoMappingExtentions
{
    public static CategoriaDTO? ToCategoriaDTO(this Categoria categoria)
    {
        if (categoria is null)
            return null;

        return new CategoriaDTO()
        {
            Nome = categoria.Nome,
            ImagemUrl = categoria.ImagemUrl,
            CategoriaId = categoria.CategoriaId,
        };
    }

    public static Categoria? ToCategoria(this CategoriaDTO categoriaDTO)
    {
        if (categoriaDTO is null)
            return null;

        return new Categoria()
        {
            Nome = categoriaDTO.Nome,
            ImagemUrl = categoriaDTO.ImagemUrl,
            CategoriaId = categoriaDTO.CategoriaId,
        };
    }

    public static IEnumerable<CategoriaDTO> ListCategoriaDTO(this IEnumerable<Categoria> categorias)
    {
        if (categorias is null || !categorias.Any())
            return new List<CategoriaDTO>();

        return categorias.Select(c => new CategoriaDTO
        {
            Nome = c.Nome,
            ImagemUrl = c.ImagemUrl,
            CategoriaId = c.CategoriaId,
        });;
    }
}
