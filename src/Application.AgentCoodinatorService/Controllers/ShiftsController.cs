using Application.AgentCoodinatorService.Data;
using Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.AgentCoodinatorService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShiftsController : ControllerBase
	{
		private TimeSpan WorkingHoursStartTime = new TimeSpan(8, 0, 0);
		private TimeSpan EveningShiftStartTime = new TimeSpan(16, 0, 0);
		private TimeSpan NightShiftStartTime = new TimeSpan(0, 0, 0);
		private const int MaxConcurrentCount = 10;
		private const double MaxQueueTheshold = 1.5;

		private readonly ILogger<ShiftsController> _logger;
		private IServiceProvider _sp;
		private ConnectionFactory _factory;
		private readonly IConnection _connection;
		private IModel _channel;
		private readonly IDistributedCache _distributedCache;
		private const string QueueMaxCountCacheKey = "QueueMaxCount";

		public ShiftsController(ILogger<ShiftsController> logger, IServiceProvider sp, IDistributedCache distributedCache)
		{
			_logger = logger;
			_sp = sp;
			_factory = new ConnectionFactory() { HostName = "localhost" };
			_connection = _factory.CreateConnection();
			_logger = logger;
			_sp = sp;
			_channel = _connection.CreateModel();
			_distributedCache = distributedCache;
		}

		[HttpPost("Start")]
		public async Task<IActionResult> StartShift([FromBody] ShiftStartDto shiftStartDto)
		{

			var concurrentChatCount = 0.0;

			// get team
			// get team members
			using (var db = new AgentCoordinatorDbContext())
			{
				var team = db.Teams.Include(o => o.Agents).ThenInclude(o => o.Seniority).FirstOrDefault(o => o.Shift == shiftStartDto.Shift);

				if (team == null)
				{
					return NotFound("Not found team");
				}

				var availableAgents = team.Agents.ToList();
				if (team.IsOverflow)
				{
					var overflowTeam = db.Teams.Include(o => o.Agents).ThenInclude(o => o.Seniority).FirstOrDefault(o => o.Shift == Shift.None);
					availableAgents.AddRange(overflowTeam.Agents);
				}


				// create queues
				foreach (var agent in availableAgents)
				{
					concurrentChatCount += agent.Seniority.Efficiency * MaxConcurrentCount;
					_channel.QueueDeclare(queue: agent.Name.ToUpper(), durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>() { ["x-max-length"] = Convert.ToInt32( agent.Seniority.Efficiency * MaxConcurrentCount), ["x-overflow"] = "reject-publish" });
					
				}





				// update database with data

				// update max count in shared database (redis)
				var capacity = Math.Floor( concurrentChatCount * MaxQueueTheshold);				
				await _distributedCache.SetStringAsync(QueueMaxCountCacheKey, capacity.ToString());
				var a = await _distributedCache.GetStringAsync(QueueMaxCountCacheKey);

			}

			return Created("", null);
		}


		[HttpPost("Stop")]
		public async Task StopShift([FromBody] ShiftStartDto shiftStartDto)
		{
			// reset max count in shared database
			var capacity = 0.0;
			await _distributedCache.SetStringAsync(QueueMaxCountCacheKey, capacity.ToString());


			// remove queues
			using (var db = new AgentCoordinatorDbContext())
			{
				var team = db.Teams.Include(o => o.Agents).ThenInclude(o => o.Seniority).FirstOrDefault(o => o.Shift == shiftStartDto.Shift);
				
				var availableAgents = team.Agents.ToList();
				if (team.IsOverflow)
				{
					var overflowTeam = db.Teams.Include(o => o.Agents).ThenInclude(o => o.Seniority).FirstOrDefault(o => o.Shift == Shift.None);
					availableAgents.AddRange(overflowTeam.Agents);
				}

				foreach (var agent in availableAgents)
				{
					_channel.QueueDelete(agent.Name.ToUpper(), false, false);
				}
			}
		}
	}
}
