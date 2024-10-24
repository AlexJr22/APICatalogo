using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APICatalogo.Controllers
{
    [Route("api/v{version:apiVersion}/test")]
    [ApiController]
    [ApiVersion("2.0")]
    public class ControllerTestV2 : ControllerBase
    {
        [HttpGet]
        public string GetVersion()
        {
            return "TestV2 - GET - Api versão 2.0";
        }
    }
}
