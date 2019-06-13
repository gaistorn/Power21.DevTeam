using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PES.Toolkit.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Service.MapReducingService
{
    public class MongoMapReduceHostService : IHostedService, IDisposable
    {
        readonly ILogger logger;
        readonly MongoMapReduceConfig map_reduce_config;
        readonly MongoDB.Driver.MongoClient client;
        readonly Timer[] taskTimers;
        public MongoMapReduceHostService(ILoggerFactory loggerFactory, MongoMapReduceConfig config)
        {
            logger = loggerFactory.CreateLogger("MongoMapReduce");
            map_reduce_config = config;
            taskTimers = new Timer[map_reduce_config.MapReduceTasks.Length];
            client = new MongoDB.Driver.MongoClient(config.ConnectionString);

        }

        private void DoMapreduce(object state)
        {
            try
            {
                MongoMapReduceTask map_reduce_task = (MongoMapReduceTask)state;
                logger.LogInformation($"Mongo MapReduce is working. {map_reduce_task.ResultDatabase}.{map_reduce_task.ResultCollection}.mapreduce()");

                string mapFile = File.ReadAllText(map_reduce_task.GetMapFunctionFileName("MapReduceFunction"));
                string ReduceFile = File.ReadAllText(map_reduce_task.GetReduceFunctionFileName("MapReduceFunction"));
                var db = client.GetDatabase(map_reduce_task.Database);
                var collections = db.GetCollection<BsonDocument>(map_reduce_task.Collection);
                var options = new MapReduceOptions<BsonDocument, BsonDocument>();
                options.JavaScriptMode = true;
                options.OutputOptions.ToJson();
                options.OutputOptions = MapReduceOutputOptions.Merge(map_reduce_task.ResultCollection, map_reduce_task.ResultDatabase);
                var asyncResults = collections.MapReduce<BsonDocument>(mapFile, ReduceFile, options);
                if(asyncResults != null)
                {
                    logger.LogInformation($"Mongo MapReduce is completed. {map_reduce_task.ResultDatabase}.{map_reduce_task.ResultCollection}.mapreduce()");
                }
            }
            catch(Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        public void Dispose()
        {
            foreach (Timer t in taskTimers)
                t.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Mongo MapReduce service is starting.");
            int idx = 0;
            foreach (MongoMapReduceTask task in map_reduce_config.MapReduceTasks)
            {
                taskTimers[idx++] = new Timer(DoMapreduce, task, TimeSpan.Zero, task.Interval);
            }
            return Task.CompletedTask;

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogWarning("Mongo Mapreduce service is stop.");
            foreach (Timer t in taskTimers)
                t.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;

        }
    }
}
