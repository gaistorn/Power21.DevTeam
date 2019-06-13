using System;
using System.Collections.Generic;
using System.Text;

namespace PES.Toolkit.Config
{
    public class MqttSubscribeConfig
    {
        public string BindAddress { get; set; } = "localhost";
        public ushort Port { get; set; } = 1884;
        public string Topic { get; set; } = "/";
        public byte QoSLevel { get; set; } = 0;
        public string ClientId { get; set; }

        public TimeSpan RecordInterval { get; set; } = TimeSpan.FromMinutes(1);
    }
}
