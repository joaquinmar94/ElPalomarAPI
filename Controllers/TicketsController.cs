using LibUsbDotNet.LibUsb;
using Microsoft.AspNetCore.Mvc;

namespace ElPalomar.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TicketsController : ControllerBase
	{

		private readonly ILogger<TicketsController> _logger;

		public TicketsController(ILogger<TicketsController> logger)
		{
			_logger = logger;
		}

		[HttpGet("cashlogy-connect")]
		public async Task<ActionResult<Object>> CheckConnectionCashLogy()
		{
			
		}
	}
}