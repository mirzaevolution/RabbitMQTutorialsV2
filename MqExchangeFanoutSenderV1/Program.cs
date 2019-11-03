using System;
using System.Text;
using RabbitMQ.Client;
namespace MqExchangeFanoutSenderV1
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
                        channel.ExchangeDeclare(
                                exchange: "fanout_exchange",
                                type: ExchangeType.Fanout
                            );
                        foreach(string message in messages)
                        {
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            IBasicProperties basicProperties = channel.CreateBasicProperties();
                            basicProperties.Persistent = true;
                            Console.WriteLine($"Processing `{message}`");
                            channel.BasicPublish(
                                    exchange: "fanout_exchange",
                                    routingKey: "",
                                    basicProperties: basicProperties,
                                    body: messageBytes
                                );
                        }
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
