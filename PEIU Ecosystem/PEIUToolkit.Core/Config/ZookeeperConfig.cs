using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PES.Toolkit.Config
{
    public class ZookeeperConfig
    {
        /// <summary>
        /// when access to zookeeper cluster, 192.168.0.103:2181,192.168.0.103:2182,192.168.0.103:2183,192.168.0.103:2184,192.168.0.103:2185
        /// </summary>
        public string ConnectionString { get; set; } = "localhost:2181";
        public string NodeName { get; set; } = "DefaultNode";
        public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
        public ZookeeperConfig() { }
    }
}
