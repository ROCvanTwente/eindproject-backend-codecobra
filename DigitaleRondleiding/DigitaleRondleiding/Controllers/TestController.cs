using Microsoft.AspNetCore.Mvc;

namespace DigitaleRondleiding.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "API is working!", status = "success" });
        }

        [HttpPost]
        public IActionResult Post([FromBody] TestRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required");

            return Ok(new { message = $"Received: {request.Name}", status = "success" });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok(new { id = id, message = $"Test data with ID {id}" });
        }
    }

    public class TestRequest
    {
        public string Name { get; set; }
    }
}
