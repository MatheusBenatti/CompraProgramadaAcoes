using Microsoft.AspNetCore.Mvc;

namespace CompraProgramadaAcoes.Api.Controllers
{
  [ApiController]
  [Route("debug")]
  public class DebugController : ControllerBase
  {
    [HttpGet("redis")]
    public async Task<IActionResult> TestRedis([FromServices] ICacheService cache)
    {
      await cache.SetAsync("test", "ok");
      var value = await cache.GetAsync("test");
      return Ok(value);
    }

    [HttpGet("kafka")]
    public async Task<IActionResult> TestKafka([FromServices] IMessagePublisher publisher)
    {
      await publisher.PublishAsync("test-topic", "Mensagem teste");
      return Ok("Mensagem enviada");
    }

    [HttpGet("app")]
    public IActionResult TestApp()
    {
      return Ok(new { 
        status = "Healthy",
        timestamp = DateTime.UtcNow,
        application = "CompraProgramadaAcoes.Api",
        version = "1.0.0"
      });
    }
  }
}
