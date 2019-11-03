using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace MqExchangeFanoutReceiverV1
{
    class Program
    {
        static void Listen()
        {
            try
            {
                IConnectionFactory connectionFactory = new ConnectionFactory()
                {
                    HostName = "localhost"
                };
                using(IConnection connection = connectionFactory.CreateConnection())
                {
                    using(IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(
                             exchange: "fanout_exchange",
                             type: ExchangeType.Fanout
                         );

                        string randomQueueName = channel.QueueDeclare().QueueName;

                        channel.QueueBind(
                                queue: randomQueueName,
                                exchange: "fanout_exchange",
                                routingKey: "",
                                arguments: null
                            );
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
                            string message = Encoding.UTF8.GetString(payload.Body);
                            Console.WriteLine($"\nProcessing message `{message}` [queue: {randomQueueName}]");
                            Thread.Sleep(2000);
                            Console.WriteLine($"Message: `{message}` was processed");
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
            Listen();
        }
    }
}
