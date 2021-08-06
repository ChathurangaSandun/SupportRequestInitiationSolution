using Application.Common;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.ChatApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatsController : ControllerBase
	{

		private readonly IPublishEndpoint _endpoint;
		private readonly ApplicationSettings _appsettings;

		public ChatsController(IPublishEndpoint endpoint, ApplicationSettings appsettings)
		{
			_appsettings = appsettings;
			_endpoint = endpoint;
		}


		[HttpPost]
		public async Task<ActionResult> Create([FromBody] SupportRequest supportRequest)
		{
			var messageId = Guid.NewGuid();
			await _endpoint.Publish<ISupportRequestCreatedMessage>(new SupportRequestCreatedMessage()
			{
				MessageId = messageId,
				SupportRequest = supportRequest,
				CreatedDate = DateTime.Now
			});



			return Created("", messageId);

		}
	}
}
