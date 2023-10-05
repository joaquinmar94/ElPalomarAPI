using LibUsbDotNet.LibUsb;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Net;
using ElPalomar.Context;
using ElPalomar.Models;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ElPalomar.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TicketsController : ControllerBase
	{

		private readonly ILogger<TicketsController> _logger;
		private readonly ElPalomarDbContext _context;

		public TicketsController(ILogger<TicketsController> logger,
			ElPalomarDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		[HttpGet("cashlogy-connect")]
		public async Task<ActionResult<string>> CheckConnectionCashLogy(string command)
		{
			try
			{
				using (TcpClient client = new TcpClient("127.0.0.1", 8092))
				{
					using (NetworkStream strem = client.GetStream())
					{
						byte[] data = Encoding.ASCII.GetBytes(command);
						strem.Write(data, 0, data.Length);
						byte[] buffer = new byte[1024];
						int bytesRead = strem.Read(buffer, 0, buffer.Length);

						if (bytesRead > 0)
						{
							string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
							return "Datos recibidos: " + dataReceived;
						}

						StreamReader reader = new StreamReader(strem);
						string response = reader.ReadToEnd();
						strem.Close();
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

		[HttpPost("{ticketId}/payment")]
		public async Task<ActionResult<string>> TicketPayment(int ticketId)
		{

			//Busco el ticket en base de datos.
			var data = _context.Tickets.Where(x => x.Id == ticketId).FirstOrDefault();

			if (data != null)
			{
				//Busco el importe del ticket.
				//decimal ticketAmount = data.Net;
				//Envio instrucción a cashlogi para que cobre el importe del ticket.
				//string instructionPayment = $"#C#00001#1#{ticketAmount}#0#0#0#0#0#1#0#0#";
				//string responseCashLogi = SendCashlogiInstruction(instructionPayment);

				return JsonConvert.SerializeObject(data, Formatting.Indented);
			}

			return "error";
		}
	}
}