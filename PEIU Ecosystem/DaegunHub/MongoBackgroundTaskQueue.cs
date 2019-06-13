using PES.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DaegunHub
{
    public interface IBackgroundMongoTaskQueue
    {
        void QueueBackgroundWorkItem(DaegunPacket workItem);
        Task<DaegunPacket> DequeueAsync(CancellationToken cancellationToken);
    }

    public class MongoBackgroundTaskQueue : IBackgroundMongoTaskQueue
    {
        private ConcurrentQueue<DaegunPacket> _workItems =
        new ConcurrentQueue<DaegunPacket>();
        MongoDB.Driver.MongoClient client;
        private SemaphoreSlim _signal = new SemaphoreSlim(0);
        
        public MongoBackgroundTaskQueue(MongoDB.Driver.MongoClient _client)
        {
            client = _client;
        }

        public async Task<DaegunPacket> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;

        }

        public void QueueBackgroundWorkItem(DaegunPacket workItem)
        {
         //   DaegunPacket workItem = new DaegunPacket(client, packetStruct);
            _workItems.Enqueue(workItem);
            _signal.Release();

        }
    }
}
