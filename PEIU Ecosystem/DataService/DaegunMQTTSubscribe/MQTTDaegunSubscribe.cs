using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using PES.Models;
using PES.Toolkit.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PES.Service.DataService
{
    public class MQTTDaegunSubscribe
    {
        readonly MqttSubscribeConfig mqttOptions;
        readonly IBackgroundMongoTaskQueue queue;
        readonly ILogger<MQTTDaegunSubscribe> logger;
        IMqttClient client;
        public MQTTDaegunSubscribe(ILoggerFactory loggerFactory, IBackgroundMongoTaskQueue taskQueue, MqttSubscribeConfig mqtt_config)
        {
            mqttOptions = mqtt_config;
            queue = taskQueue;
            logger = loggerFactory.CreateLogger<MQTTDaegunSubscribe>();
            StartSubscribe();
        }

        private async void StartSubscribe()
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

            client = new MqttFactory().CreateMqttClient();
            client.ApplicationMessageReceived += ManagedClient_ApplicationMessageReceived; ;
            client.Connected += ManagedClient_Connected;
            client.Disconnected += Client_Disconnected;

            try
            {
                var result =  await client.ConnectAsync(ClientOptions);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "### CONNECTING FAILED ###" + Environment.NewLine + exception);
            }
        }

        private async void Client_Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            logger.LogInformation($"### DISCONNECTED FROM SERVER. TRY CONNECT AFTER {mqttOptions.RecordInterval} ###");
            await Task.Delay(mqttOptions.RecordInterval);

            try
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
                await client.ConnectAsync(ClientOptions);
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "### RECONNECTING FAILED ###");
            }
        }

        private async void ManagedClient_Connected(object sender, MqttClientConnectedEventArgs e)
        {
            logger.LogInformation($"### CONNECTED WITH SERVER (TOPIC FILTER:{mqttOptions.Topic}) ###");
            await client.SubscribeAsync(new TopicFilterBuilder().WithTopic(mqttOptions.Topic).Build());
            
        }

        //Dictionary<int, DateTime> lastRecordTime = new Dictionary<int, DateTime>();
            

        private void ManagedClient_ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
        {
            logger.LogInformation($"RECEIVED DAEGUN :from({e.ClientId}) t({e.ApplicationMessage.Topic}) QoS({e.ApplicationMessage.QualityOfServiceLevel}) size({e.ApplicationMessage.Payload.Length})");
            byte[] pay_load = e.ApplicationMessage.Payload;
            byte[] new_fileArray = new byte[pay_load.Length + 8];
            Array.Copy(pay_load, new_fileArray, pay_load.Length);
            DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(new_fileArray);
            packet.timestamp = DateTime.Now;
            queue.QueueBackgroundWorkItem(packet);
            //logger.LogInformation("Store Daegun Packet");
            //if(lastRecordTime.ContainsKey(packet.sSiteId) == false)
            //{
            //    lastRecordTime.Add(packet.sSiteId, DateTime.MinValue);
            //}

            //if (DateTime.Now > lastRecordTime[packet.sSiteId])
            //{
               
            //    lastRecordTime[packet.sSiteId] = DateTime.Now.Add(mqttOptions.RecordInterval);
            //}
            
        }
    }
}
