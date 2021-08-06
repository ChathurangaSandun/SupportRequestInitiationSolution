using Application.Common;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Coodinator
{
	public class SupportRequestCreatedConsumer : IConsumer<ISupportRequestCreatedMessage>
	{

        private readonly IServiceProvider _serviceProvider;
		public SupportRequestCreatedConsumer(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task Consume(ConsumeContext<ISupportRequestCreatedMessage> context)
		{
            try
            {
                Console.WriteLine($"{context.Message.CreatedDate} - {context.Message.MessageId}");


                var publisher =(IPublishEndpoint) _serviceProvider.GetService (typeof(IPublishEndpoint));

                var supportCreatedMessage = new SupportRequestCreatedMessage()
                {
                    MessageId = new Guid(),
                    SupportRequest = context.Message.SupportRequest,
                    CreatedDate = DateTime.Now
                };

                //await publisher.Publish(supportCreatedMessage);

                await context.RespondAsync<SupportRequestAccepted>(new
                {
                    Value = $"Received: {context.Message.MessageId}"
                });
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }
	}
}
