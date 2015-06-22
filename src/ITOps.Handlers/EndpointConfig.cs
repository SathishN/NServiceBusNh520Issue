using System;
using System.Data.SqlClient;
using System.Reflection;
using ITOps.Messages;
using ITOps.Messages.Commands;
using NHibernate.Cfg;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.Persistence;
using StructureMap;
using NServiceBus;

namespace ITOps.Handlers 
{
    using NServiceBus;

	/*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://particular.net/articles/profiles-for-nservicebus-host
	*/
    public class EndpointConfig : AsA_Server, IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration config)
        {
            log4net.Config.XmlConfigurator.Configure();

            LogManager.Use<NServiceBus.Log4Net.Log4NetFactory>();

            var container = new Container(cfg => { });

            config.AssembliesToScan(new [] {typeof (NotificationCommand).Assembly, typeof (ConsoleNotificationHandler).Assembly});

            config.EndpointName("ITOps.Notification");
            config.Conventions()
                .DefiningMessagesAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Messages"))
                .DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Events"))
                .DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Messages.Commands"));
            
            config.UseContainer<StructureMapBuilder>(cfg => cfg.ExistingContainer(container));

            //config.UsePersistence<InMemoryPersistence>();

            //based on http://docs.particular.net/nservicebus/nhibernate/configuration#configuringnhibernate-v5_2_x-N
            //Use NHibernate for all persistence concerns
            config.UsePersistence<NHibernatePersistence>();

            config.UseSerialization<XmlSerializer>();
            config.EnableFeature<TimeoutManager>();
            config.EnableFeature<SecondLevelRetries>();
        }
    }

    public class CreateDb : IWantToRunBeforeConfigurationIsFinalized
    {
        public void Run(Configure config)
        {
            using (var connection = new SqlConnection(@"server=(localdb)\v11.0"))
            {
                connection.Open();

                var command = new SqlCommand("Select top 1 '1' from sys.databases  Where name = 'NServiceBus'", connection);
                var result = command.ExecuteScalar();

                if (result != null && result.ToString() == "1")
                {
                    return;
                }

                string sql = string.Format(@"
                        CREATE DATABASE
                            [NServiceBus]
                        ON PRIMARY (
                           NAME=NServiceBus_data,
                           FILENAME = '{0}\NServiceBus_data.mdf'
                        )
                        LOG ON (
                            NAME=NServiceBus_log,
                            FILENAME = '{0}\NServiceBus_log.ldf'
                        )",
                        AppDomain.CurrentDomain.BaseDirectory
                    );

                command = new SqlCommand(sql, connection);
                command.ExecuteNonQuery();
            }
        }
    }
}