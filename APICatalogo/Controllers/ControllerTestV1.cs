using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [Route("api/v{version:apiVersion}/test")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ControllerTestV1 : ControllerBase
    {
        [HttpGet]
        public string GetVervion()
        {
            return "TestV1 - GET - Api versão 1.0";
        }
    }
}
