using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace MqExchangeDirectSenderV1
{
    class Program
    {
        static void Send(string group, string[] messages)
        {
            try
            {
                IConnectionFactory connectionFactory = new ConnectionFactory
                {
                    HostName = "localhost"
                };
                using(IConnection connection = connectionFactory.CreateConnection())
                {
                    using(IModel channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(
                                exchange: "direct_exchange",
                                type: ExchangeType.Direct
                            );
                        foreach(string message in messages)
                        {
                            Console.WriteLine($"Sending `{message}` to {group} to group: {group}");
                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            IBasicProperties basicProperties = channel.CreateBasicProperties();
                            basicProperties.Persistent = true;
                            channel.BasicPublish(
                                    exchange: "direct_exchange",
                                    routingKey: group,
                                    basicProperties: basicProperties,
                                    body: messageBytes
                                );

                        }
                        Console.WriteLine("All messages sent successfully");
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
            if (args.Length < 2)
            {
                Console.WriteLine("MqExchangeDirectSenderV1.exe <group_name> <message 1> <message 2> <message N>");

            }
            string groupName = args.First();
            string[] messages = args.Skip(1).ToArray();
            Send(group: groupName, messages: messages);
        }
    }
}
