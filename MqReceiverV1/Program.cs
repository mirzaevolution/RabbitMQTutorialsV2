using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MqReceiverV1
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
                        //It will be created only if queue doesn't exist
                        channel.QueueDeclare(
                                queue: "queue_intro",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
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
                            Console.WriteLine($"Message: `{message}` was processed");
                        };
                        channel.BasicConsume(
                                queue: "queue_intro",
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
