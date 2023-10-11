using AgoraService.Interface;
using AgoraService.Models;
using ElPalomar.Context;
using ElPalomar.Enum;
using ElPalomar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ElPalomar.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class TicketsController : ControllerBase
	{

		private readonly ILogger<TicketsController> _logger;
		private readonly ElPalomarDbContext _context;
		private readonly IAgoraAPIService _agoraService;
		private readonly IAgoraBusService _agoraBusService;

		public TicketsController(ILogger<TicketsController> logger,
			ElPalomarDbContext context,
			IAgoraAPIService agoraService,
			IAgoraBusService agoraBusService)
		{
			_logger = logger;
			_context = context;
			_agoraService = agoraService;
			_agoraBusService = agoraBusService;
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

		[HttpGet("{ticketId}/pos/{posId}/user/{userId}/payment")]
		public async Task<ActionResult<ResultModel>> TicketPayment(int ticketId, int posId, int userId)
		{
			try
			{
				//Busco el ticket en base de datos.
				var data = _context.Tickets.Include(t => t.TicketLines).ThenInclude(tl => tl.TicketLineAddins).FirstOrDefault(t => t.Id == ticketId);

				if (data != null)
				{
					//Busco el importe del ticket.
					decimal ticketAmount = Math.Round(data.Net, 2);
					int amountIntValue = (int)(ticketAmount * 100);

					//Envio instrucción a cashlogi para que cobre el importe del ticket.
					string instructionPayment = $"#C#00001#1#{amountIntValue}#0#0#0#0#0#1#0#0#";
					//string responseCashLogi = SendCashlogiInstruction(instructionPayment);

					//Elimino la linea mas cara del ticket
					TicketLine ticketLineDeleted = null;
					if (data.TicketLines.Count > 1)
					{
						//Ordeno las linea del ticket de mayor a menor en base al totalAmount
						IEnumerable<TicketLine> ticketLines = data.TicketLines.OrderByDescending(tl => tl.TotalAmount);
						ticketLineDeleted = ticketLines.First();
						//Elimino la linea del TicketLine en Base de Datos
						data.TicketLines.Remove(ticketLineDeleted);

						using (var transaction = _context.Database.BeginTransaction())
						{
							data.Net = data.TicketLines.Sum(x => x.TotalAmount);
							List<TicketLine> ticketLinesList = data.TicketLines.ToList();

							for(int i = 0; i< ticketLinesList.Count; i++)
							{
								ticketLinesList[i].TicketLineIndex = i;
							}

							data.TicketLines = ticketLinesList;
							var actualChangeCount = _context.SaveChanges();
							if (actualChangeCount > 0)
							{
								transaction.Commit();
							}
						}

						//string deleteTicketLineResponse = await _agoraBusService.DeleteTicketLine(ticketLineDeleted.Id, ticketLineDeleted.TicketId);						
					}

					//Armo el objeto que se envia a la API de Ágora para procesar el pago del ticket
					AgoraTicketPaymentRequestModel ticketPaymentBody = new AgoraTicketPaymentRequestModel()
					{
						TicketGlobalId = data.GlobalId,
						PosId = posId,
						UserId = userId,
						Date = DateTime.Now,
						PaymentMethodId = (int)PaymentMethodEnum.Cash,
						Amount = data.Net, //Total del ticket (actualmente seria el total del ticket menos el total del ticket eliminado)
						PaidAmount = data.Net, //Aca deberia ir el monto que el cliente introdujo en el CashLogi (Ver respuesta del cashlogi)
						ChangeAmount = 0, //Cambio que dió el CashLogi (Ver respuesta o calcular cambio con data.Net y el monto introducido por el cliente)
					};

					string addPaymentResponse = await _agoraService.PostPayment(JsonConvert.SerializeObject(ticketPaymentBody));

					return new ResultModel { HttpStatusCode = HttpStatusCode.OK, Content = addPaymentResponse};
				}

				return new ResultModel { HttpStatusCode = HttpStatusCode.BadRequest, Content = "No se encontró el ticket." };
			}
			catch (Exception ex)
			{
				return new ResultModel { HttpStatusCode = HttpStatusCode.InternalServerError, Content = ex.Message };
			}
			
		}
	}
}