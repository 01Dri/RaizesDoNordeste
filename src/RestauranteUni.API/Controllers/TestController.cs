using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestauranteUni.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TestController : ControllerBase
    {
        public TestController()
        {
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Olá");
        }
    }
}
