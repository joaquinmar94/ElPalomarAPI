using LibUsbDotNet.LibUsb;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Text;

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
		public async Task<ActionResult<string>> CheckConnectionCashLogy()
		{
			try
			{
				using (TcpClient client = new TcpClient("192.168.10.9", 8092))
				{
					using (NetworkStream strem = client.GetStream())
					{
						byte[] data = Encoding.ASCII.GetBytes("#I#");
						strem.Write(data, 0, data.Length);

						StreamReader reader = new StreamReader(strem);
						string response = reader.ReadToEnd();
						client.Close();
						return response;
					}
				}
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}
	}
}