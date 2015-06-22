using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITOps.Messages;
using ITOps.Messages.Commands;
using NServiceBus;

namespace ITOps.Handlers
{
    public class ConsoleNotificationHandler : IHandleMessages<NotificationCommand>
    {
        public void Handle(NotificationCommand message)
        {
            Console.WriteLine("ConsoleNotificationHandler : " + message.Message);
            throw new ApplicationException("Message Error");
        }
    }
}
