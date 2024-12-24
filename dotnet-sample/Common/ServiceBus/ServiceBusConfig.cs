using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ServiceBus
{
    public class ServiceBusConfig
    {
        public string FullyQualifiedNamespace { get; set; }

        public Queues Queues { get; set; }
        public Topics Topics { get; set; }

        public ServiceBusConfig()
        {
            Queues = new Queues();
            Topics = new Topics();
        }
    }

    public class Queues
    {
        public string IdentityEvents { get; set; }
        public string SendComms { get; set; }
        public string DeleteAccount { get; set; }
        public string ProcessCompromisedFile { get; set; }
        public string DeleteStrongAuth { get; set; }
    }

    public class Topics
    {
        public string UserIdentityStatus { get; set; }
        public string UserAccountStatus { get; set; }
    }
}
