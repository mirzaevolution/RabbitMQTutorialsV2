using System;
using System.Linq;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace MsExchangeDirectReceiverV2
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
                using (IConnection connection = connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(
                                exchange: "direct_exchange",
                                type: ExchangeType.Direct
                            );
                        foreach (string groupName in groupNames)
                        {
                            string queueName = groupName;

                            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: true, arguments: null);
                            channel.BasicQos(
                                prefetchSize: 0,
                                prefetchCount: 1,
                                global: false
                             );
                            channel.QueueBind(
                                queue: queueName,
                                exchange: "direct_exchange",
                                routingKey: groupName,
                                arguments: null
                            );

                        }
                        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                        consumer.Registered += (sender, obj) =>
                        {
                            
                            Console.WriteLine($"*** RabbitMQ Listener {obj.ConsumerTag} Registered  ***");
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
                            Console.WriteLine($"Message: {message}");
                            channel.BasicAck(deliveryTag: payload.DeliveryTag, multiple: true);
                        };
                        foreach(string queueName in groupNames)
                        {
                            channel.BasicConsume(
                                    queue: queueName,
                                    autoAck: false,
                                    consumer: consumer
                                );
                        }
                        Console.WriteLine("Press [ENTER] to quit");
                        Console.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("MqExchangeDirectReceiverV2.exe <group_name 1> <group_name 2> <group_name N>");
            }

            Listen(groupNames: args);
        }
    }
}
