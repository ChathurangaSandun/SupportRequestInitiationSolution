using Application.AgentCoodinatorService.Data;
using Application.Common;
using Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.AgentCoodinatorService.HostedService
{
	public class TimedHostedService : IHostedService, IDisposable
	{
		private readonly ILogger<TimedHostedService> _logger;
		private Timer _timer;
		private IServiceProvider _sp;
		private ConnectionFactory _factory;
		private readonly IConnection _connection;
		private IModel _channel;
		List<ProcessAgent> processedAgents = new List<ProcessAgent>();		
		List<int> SeniorityOrder = new List<int>();
		private const int MaxConcurrentCount = 10;
		int messageCount = 0;

		public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider sp)
		{
			_logger = logger;
			_sp = sp;
			_factory = new ConnectionFactory() { HostName = "localhost" };
			_connection = _factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.QueueDeclare(queue: "SessionQueue", durable: true, exclusive: false, autoDelete: false, arguments: null/*new Dictionary<string, object>() { ["x-max-length"] = 10, ["x-overflow"] = "reject-publish" }*/);

			using (var db = new AgentCoordinatorDbContext())
			{
				var team = db.Teams.Include(o => o.Agents).ThenInclude(o => o.Seniority).FirstOrDefault(o => o.Shift == Shift.OfficeTime);
				foreach (var agent in team.Agents)
				{
					processedAgents.Add(new ProcessAgent()
					{
						Agent = agent,
						QueuueCount = 0,
						AssignCount = 0
					}
				);

				}
			}


		}

		public Task StartAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Timed Hosted Service running.");
			_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));			
			
			return Task.CompletedTask;
		}

		//TODO: make method async and await
		private void DoWork(object state)
		{


			var size = _channel.MessageCount("SessionQueue");
			var availableSlots = 30;
			if (size > 0)
			{
				for (int i = 0; i < availableSlots; i++)
				{
					var b = _channel.BasicGet(queue: "SessionQueue", autoAck: true);
					var body = b.Body.ToArray();
					var message = Encoding.UTF8.GetString(body);
					//_logger.LogInformation("Received {0}", message);

					using (var db = new AgentCoordinatorDbContext())
					{
						var request = JsonConvert.DeserializeObject<SupportRequestCreatedMessage>(message).SupportRequest;
						request.CreatedTime = DateTime.UtcNow;
						db.SupportRequests.Add(request);
						db.SaveChanges();


						// resolve queue
						// TODO: resolve shift

						SeniorityOrder = db.Seniorities.OrderBy(o => o.AssignOrder).Select(o => o.AssignOrder).ToList();

						var selectedAgent = AssignRequest();
						if (selectedAgent != null)
						{
							_logger.LogInformation("[AgentName] " + selectedAgent.Agent.Name) ;
							var requestJson = JsonConvert.SerializeObject(request);
							var requestBody = Encoding.UTF8.GetBytes(requestJson);

							_channel.BasicPublish(exchange: "", routingKey: selectedAgent.Agent.Name.ToUpper(), basicProperties: null, body: requestBody);
						}
						
					}
				}
			}

			_logger.LogInformation("Timed Hosted Service is working.");
		}


		private ProcessAgent AssignRequest()
		{
			ProcessAgent selectedAgent = null;

			// get current queue counts
			processedAgents.ForEach(process => {
				process.QueuueCount = _channel.MessageCount(process.Agent.Name.ToUpper());
			});


			foreach (var order in SeniorityOrder)
			{
				var agentList = processedAgents.Where(o => o.Agent.Seniority.AssignOrder == order).OrderBy(o => o.QueuueCount).OrderBy(o => o.AssignCount);
				
				foreach (var agent in agentList)
				{					
					if ((int)agent.QueuueCount < (agent.Agent.Seniority.Efficiency * MaxConcurrentCount) ) {
						messageCount += 1;
						agent.AssignCount = (int) messageCount; 
						return agent;
						
					}
				}
			}

			return selectedAgent;
		}

		public Task StopAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Timed Hosted Service is stopping.");
			_timer?.Change(Timeout.Infinite, 0);
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}


	internal class ProcessAgent
	{
		public Agent Agent { get; set; }
		public uint QueuueCount { get; set; }
		public int AssignCount { get; set; }
	}
}
