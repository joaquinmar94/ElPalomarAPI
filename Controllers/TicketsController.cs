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

		[HttpGet("usb-devices")]
		public async Task<ActionResult<Object>> GetDevices()
		{
			var allDevices = LibUsbDotNet.LibUsb.LibUsbDevice.AllDevices;
			var allWinUsbDevices = LibUsbDevice.AllWinUsbDevices;

			return new { allDevices, allWinUsbDevices };
		}
	}
}