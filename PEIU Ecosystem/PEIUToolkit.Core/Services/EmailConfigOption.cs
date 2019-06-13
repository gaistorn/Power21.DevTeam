using System;
using System.Collections.Generic;
using System.Text;

namespace PES.Toolkit.Services
{
    public class EmailConfigOption
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        public string FromEmail { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
