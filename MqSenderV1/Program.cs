using System;
using System.Text;
using RabbitMQ.Client;
namespace MqSenderV1
{
    class Program
    {
        static void Send(string[] messages)
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

                        //Distribute all messages
                        foreach(string message in messages)
                        {
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            Console.WriteLine($"Sending message: `{message}`");
                            channel.BasicPublish(
                                    exchange: "",
                                    routingKey: "queue_intro",
                                    basicProperties: null,
                                    body: messageBytes
                                );
                        }
                        Console.WriteLine("All messsages sent successfully");
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
            Send(args == null || (args != null && args.Length == 0) ? new[] { "Hello World" } : args);
        }
    }
}
