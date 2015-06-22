using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ITOps.Messages.Commands;
using NServiceBus;
using NServiceBus.Logging;

namespace ITOps.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            LogManager.Use<NServiceBus.Log4Net.Log4NetFactory>();

            var config = new BusConfiguration();

            config.AssembliesToScan(new[] {typeof (NotificationCommand).Assembly});
            config.EndpointName("ITOps.Notification.Client");
            config.Conventions()
                .DefiningMessagesAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Messages"))
                .DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Events"))
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Commands"));

            config.UsePersistence<InMemoryPersistence>();
            config.UseContainer<StructureMapBuilder>();
            config.UseTransport<MsmqTransport>();
            config.UseSerialization<XmlSerializer>();
            config.Transactions().Enable();

            var bus = Bus.CreateSendOnly(config);


            Console.WriteLine("Type any button to send an notification , q to Quit.");

            while (Console.ReadLine() != "q")
            {
                var notification = new NotificationCommand() {Message = "Billing complete"};
                bus.Send(notification);
                Console.WriteLine("Notification sent.");

                Console.WriteLine("Type any button to send an notification , q to Quit.");
            }
        }
    }
}
