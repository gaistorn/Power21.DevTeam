using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PES.Toolkit.Services
{
    public interface IZookeeperHostService
    {
        ZooKeeper GetZooKeeper();
    }
    public class ZooKeeperHostService : IZookeeperHostService
    {
        private readonly ZooKeeper zk;
        public ZooKeeperHostService(IOptions<Config.ZookeeperConfig> config, Watcher @watcher)
        {
            zk = new ZooKeeper(config.Value.ConnectionString, config.Value.SessionTimeout.Milliseconds, @watcher);
        }

        public ZooKeeper GetZooKeeper()
        {
            return zk;
        }
    }
}
