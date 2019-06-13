using DaegunHub;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using PES.Models;
using PES.Toolkit.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Power21.PEIUEcosystem.DataHub
{
    public class MQTTBackgroundSubscribeService : BackgroundService
    {
        readonly MqttSubscribeConfig mqttOptions;
        readonly IBackgroundMongoTaskQueue queue;
        readonly ILogger<MQTTBackgroundSubscribeService> logger;
        public MQTTBackgroundSubscribeService(ILoggerFactory loggerFactory, IBackgroundMongoTaskQueue taskQueue, MqttSubscribeConfig mqtt_config)
        {
            mqttOptions = mqtt_config;
            queue = taskQueue;
            logger = loggerFactory.CreateLogger<MQTTBackgroundSubscribeService>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ClientOptions = new MqttClientOptions
            {
                ClientId = mqttOptions.ClientId,
                ChannelOptions = new MqttClientTcpOptions
                {
                    Server = mqttOptions.BindAddress,
                    Port = mqttOptions.Port
                }
            };

            var managedClient = new MqttFactory().CreateMqttClient();
            managedClient.ApplicationMessageReceived += ManagedClient_ApplicationMessageReceived;
            managedClient.Connected += ManagedClient_Connected;
            while (!stoppingToken.IsCancellationRequested)
            {
                //managedClient.sub
            }
            throw new NotImplementedException();
        }

        private void ManagedClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            
        }

        private void ManagedClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            logger.LogInformation($"RECEIVED DAEGUN :from({e.ClientId}) t({e.ApplicationMessage.Topic}) QoS({e.ApplicationMessage.QualityOfServiceLevel}) size({e.ApplicationMessage.Payload.Length})");
            byte[] pay_load = e.ApplicationMessage.Payload;
            byte[] new_fileArray = new byte[pay_load.Length + 8];
            Array.Copy(pay_load, new_fileArray, pay_load.Length);
            DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(new_fileArray);
            packet.timestamp = DateTime.Now;
            queue.QueueBackgroundWorkItem(packet);
            logger.LogInformation("Queuying Daegun Packet");
        }
    }
}
