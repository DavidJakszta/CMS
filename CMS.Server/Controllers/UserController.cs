using CMS.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase

    {
        [HttpPost]
        public IActionResult CreateUser([FromBody] User user)
        {
            return null;
        }

        [HttpGet]
        public IActionResult ReadUser([FromBody] string Id)
        {
            return null;
        }
    }
}
