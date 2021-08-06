using Application.Common;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace Application.Coodinator
{
	class Program
	{
		static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
		   .UseWindowsService()
		   .ConfigureAppConfiguration((hostingContext, config) => {

			   config.AddConfiguration(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
		   })
			.ConfigureServices((hostContext, services) =>
			{

				var queueSettings = new QueueSettings();


				hostContext.Configuration.GetSection("QueueSettings").Bind(queueSettings);

				services.AddSingleton(queueSettings);


				services.RegisterQueueServices(hostContext);
				//services.AddHostedService<Worker>();
				services.AddHostedService<TimedHostedService>();
			});


	}

	public static class QueueConsumerExtension{
		public static IServiceCollection RegisterQueueServices(this IServiceCollection services, HostBuilderContext context)
		{
			var queueSettings = new QueueSettings();
			context.Configuration.GetSection("QueueSettings").Bind(queueSettings);

			services.AddMassTransit(c =>
			{
				c.AddConsumer<SupportRequestCreatedConsumer>();
			});

			services.AddSingleton(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
			{
				cfg.Host(queueSettings.HostName, queueSettings.VirtualHost, h => {
					h.Username(queueSettings.UserName);
					h.Password(queueSettings.Password);
				});
			}));

			return services;
		}
	}

}
