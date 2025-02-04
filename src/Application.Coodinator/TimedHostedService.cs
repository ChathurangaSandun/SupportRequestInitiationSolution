﻿using Application.Common;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Coodinator
{
	public class TimedHostedService : IHostedService, IDisposable
	{		
		private readonly ILogger<TimedHostedService> _logger;
		private Timer _timer;
		private IServiceProvider _sp;
		private ConnectionFactory _factory;
		private IConnection _connection;
		private IModel _channel;

		public TimedHostedService(ILogger<TimedHostedService> logger, IServiceProvider sp)
		{
			_logger = logger;
			_sp = sp;
			_factory = new ConnectionFactory() { HostName = "localhost" };
			_connection = _factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.QueueDeclare(queue: "SessionQueue", durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object>() { ["x-max-length" ] = 10 , ["x-overflow"] = "reject-publish"});
		}

		public Task StartAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Timed Hosted Service running.");
			_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

			return Task.CompletedTask;
		}

		private void DoWork(object state)
		{
			var size = _channel.MessageCount("SessionQueue");
			if (size > 0)
			{
				var b = _channel.BasicGet(queue: "SessionQueue", autoAck: true);
				var body = b.Body.ToArray();
				var message = Encoding.UTF8.GetString(body);
				Console.WriteLine("Received {0}", message);
			}

			_logger.LogInformation("Timed Hosted Service is working.");
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
}
