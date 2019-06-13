using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using PES.Toolkit.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaegunHub
{
    public class BackgroundMqttService : BackgroundService
    {
        private readonly ILogger logger;
        private readonly MqttFactory factory;
        private readonly IMqttClient client;
        public BackgroundMqttService(ILoggerFactory loggerFactory, MqttSubscribeConfig config)
        {
            factory = new MqttFactory();
            client = factory.CreateMqttClient();
            var client_options = new MqttClientOptions
            {
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = config.BindAddress,
                    Port = config.Port,
                }
            };


        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
