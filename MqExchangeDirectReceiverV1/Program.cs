using System;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace MqExchangeDirectReceiverV1
{
    class Program
    {
        static void Listen(string[] groupNames)
        {
            try
            {
                IConnectionFactory connectionFactory = new ConnectionFactory
                {
                    HostName = "localhost"
                };
                using(IConnection connection = connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(
                                exchange: "direct_exchange",
                                type: ExchangeType.Direct
                            );
                        string randomQueueName = channel.QueueDeclare().QueueName;
                        
                        foreach(string groupName in groupNames)
                        {
                            channel.QueueBind(
                                queue: randomQueueName,
                                exchange: "direct_exchange",
                                routingKey: groupName,
                                arguments: null
                            );

                        }


                        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                        consumer.Registered += (sender, obj) =>
                        {
                            Console.WriteLine($"*** RabbitMQ Listener Registered ***");
                        };
                        consumer.Shutdown += (sender, obj) =>
                        {
                            Console.WriteLine("*** Shutting down RabbitMQ Listener ***");
                        };
                        consumer.Received += (sender, payload) =>
                        {
                            string groupName = payload.RoutingKey;
                            string message = Encoding.UTF8.GetString(payload.Body);
                            Console.WriteLine($"\nGroup: {groupName}");
                            Console.WriteLine($"Queue: {randomQueueName}");
                            Console.WriteLine($"Message: {message}");
                        };
                        channel.BasicConsume(
                                queue: randomQueueName,
                                autoAck: true,
                                consumer: consumer
                            );
                        Console.WriteLine("Press [ENTER] to quit");
                        Console.ReadLine();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("MqExchangeDirectReceiverV1.exe <group_name 1> <group_name 2> <group_name N>");
            }

            Listen(groupNames: args);
        }
    }
}
