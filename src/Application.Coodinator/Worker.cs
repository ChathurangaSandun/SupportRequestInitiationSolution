using Application.Common;
using MassTransit;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Coodinator
{
    public class Worker : BackgroundService
    {
        
        private readonly IBusControl _busControl;
        private readonly IServiceProvider _serviceProvider;
        private readonly QueueSettings _queueSettings;
        public Worker(IServiceProvider serviceProvider, IBusControl busControl, QueueSettings queueSettings)
        {            
            _busControl = busControl;
            _serviceProvider = serviceProvider;
            _queueSettings = queueSettings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                
                var productChangeHandler = _busControl.ConnectReceiveEndpoint(_queueSettings.QueueName, x =>
                {
                    x.Consumer<SupportRequestCreatedConsumer>(_serviceProvider);
                });

                await productChangeHandler.Ready;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
