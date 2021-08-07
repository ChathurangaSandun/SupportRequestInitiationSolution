using Application.Common;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.ChatApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ChatsController : ControllerBase
	{


		private readonly ApplicationSettings _appsettings;
		private readonly IDistributedCache _distributedCache;
		private const string QueueMaxCountCacheKey = "QueueMaxCount";

		public ChatsController(ApplicationSettings appsettings, IDistributedCache distributedCache)
		{
			_appsettings = appsettings;
			_distributedCache = distributedCache;
		}


		[HttpPost]
		public async Task<ActionResult> Create([FromBody] SupportRequest supportRequest)
		{

			var queueMaxCount = await _distributedCache.GetStringAsync(QueueMaxCountCacheKey);
			if (string.IsNullOrWhiteSpace(queueMaxCount))
			{
				return NotFound("Cannot found required configs");
			}

			var factory = new ConnectionFactory() { HostName = "localhost" };
			using (var connection = factory.CreateConnection())

			using (var channel = connection.CreateModel())
			{
				for (int i = 0; i < 100; i++)
				{

					var queueCount = channel.MessageCount("SessionQueue");
					if (int.Parse(queueMaxCount) < queueCount + 1)
					{
						return NotFound("Queue is not available");
					}


					supportRequest.User = i.ToString();
					supportRequest.Id = Guid.NewGuid();
					var supportCreatedMessage = new SupportRequestCreatedMessage()
					{
						MessageId = Guid.NewGuid(),
						SupportRequest = supportRequest,
						CreatedDate = DateTime.UtcNow
					};

					var json = JsonConvert.SerializeObject(supportCreatedMessage);
					var body = Encoding.UTF8.GetBytes(json);


					channel.BasicPublish(exchange: "",
										 routingKey: "SessionQueue",
										 basicProperties: null,
										 body: body);

					Thread.Sleep(1000);

				}

				return Created("", null);
			}

		}
	}
}
