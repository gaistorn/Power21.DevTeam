using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet.Server;
using MQTTnet.AspNetCore;
using MQTTnet;
using StackExchange.Redis.Extensions.Core.Configuration;
using PES.Toolkit;
using PES.Toolkit.Config;
using PES.Models;
using MQTTnet.Client;

namespace DaegunHub
{
    public class Startup
    {
        static IBackgroundMongoTaskQueue queue_service;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

#if ENABLE_MQTT_BROKER
            var mqttServerOptions = new MqttServerOptionsBuilder()
                .WithoutDefaultEndpoint()
                .Build();

            services
                .AddHostedMqttServer(mqttServerOptions)
                .AddMqttConnectionHandler()
                .AddConnections();
#endif
            MongoDB.Driver.MongoClient client = new MongoDB.Driver.MongoClient(Configuration.GetConnectionString("mongodb"));
            
            var redisConfiguration = Configuration.GetSection("redis").Get<RedisConfiguration>();
            var mqttOptions = Configuration.GetSection("MQTT").Get<MqttSubscribeConfig>();
            services.AddSingleton(mqttOptions);
            services.AddSingleton(redisConfiguration);
            services.AddSingleton<IRedisConnectionFactory, RedisConnectionFactory>();
            services.AddSingleton(client);
            queue_service = new MongoBackgroundTaskQueue(client);
            services.AddSingleton(queue_service);
            //IServiceCollection cols  = services.AddSingleton<IBackgroundMongoTaskQueue, MongoBackgroundTaskQueue>();
            //services.AddHostedService<MongoBackgroundHostService>();




            //byte[] file_array = File.ReadAllBytes("output.txt");

            //byte[] new_fileArray = new byte[file_array.Length + 8];
            //Array.Copy(file_array, new_fileArray, file_array.Length);
            //DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(new_fileArray);
            //packet.timestamp = DateTime.Now;
            //queue_service.QueueBackgroundWorkItem(packet);



            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
#if ENABLE_MQTT_BROKER
            app.UseConnections(c => c.MapConnectionHandler<MqttConnectionHandler>("/mqtt", options => {
                options.WebSockets.SubProtocolSelector = MQTTnet.AspNetCore.ApplicationBuilderExtensions.SelectSubProtocol;
            }));
#endif

            #region MQTT BROKER

#if ENABLE_MQTT_BROKER

            //app.UseMqttEndpoint();
            app.UseMqttServer(server =>
            {
                
                server.ApplicationMessageReceived += (sender, args) =>
                {
                    
                    byte[] file_array = args.ApplicationMessage.Payload;
                    Console.WriteLine($"receive: (clientid){args.ClientId} (t){args.ApplicationMessage.Topic} (length){file_array.Length}");
                    //byte[] new_fileArray = new byte[file_array.Length + 8];
                    //Array.Copy(file_array, new_fileArray, file_array.Length);
                    //DaegunPacket packet = PacketParser.ByteToStruct<DaegunPacket>(new_fileArray);
                    //packet.timestamp = DateTime.Now;

                    //queue_service.QueueBackgroundWorkItem(packet);
                    string message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                    
                };
                server.ClientConnected += (sender, args) =>
                {
                    Console.WriteLine("Connected. (clientid)" + args.ClientId);
                };
                server.ClientDisconnected += (sender, args) =>
                {
                    Console.WriteLine("Disconnected. (clientid)" + args.ClientId);
                };
                server.Started += (sender, args) =>
                {
                    Console.WriteLine("MQTT Broker is started");
                    //var msg = new MqttApplicationMessageBuilder()
                    //    .WithPayload("Mqtt is awesome")
                    //    .WithTopic("message");

                    //while (true)
                    //{
                    //    try
                    //    {
                    //        //await server..SubscribeAsync()

                    //        await server.PublishAsync(msg.Build());
                    //        msg.WithPayload("Mqtt is still awesome at " + DateTime.Now);


                    //    }
                    //    catch (Exception e)
                    //    {
                    //        Console.WriteLine(e);
                    //    }
                    //    finally
                    //    {
                    //        await Task.Delay(TimeSpan.FromSeconds(2));
                    //    }
                    //}
                };
            });
#endif
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
        }
    }
}
