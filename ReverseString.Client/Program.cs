using System;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ReverseString.Client
{
    class Program
    {
        private static void Callback(string result)
        {
            Console.WriteLine($"Result: `{result}`");
        }
        private static void Send(string input)
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
                        string result = string.Empty;
                        //create random queue
                        string queueName = channel.QueueDeclare().QueueName;

                        IBasicProperties basicProperties = channel.CreateBasicProperties();
                        basicProperties.CorrelationId = Guid.NewGuid().ToString();
                        basicProperties.ReplyTo = queueName;

                        EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (sender, payload) =>
                        {
                           if(payload.BasicProperties.CorrelationId == basicProperties.CorrelationId)
                            {
                                result = Encoding.UTF8.GetString(payload.Body);
                                Callback(result);

                            }
                        };
                        channel.BasicConsume(
                                queue: queueName,
                                autoAck: true,
                                consumer: consumer
                            );

                        Console.WriteLine($"Processing: `{input}`");
                        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                        channel.BasicPublish(
                                exchange: "",
                                routingKey: "reversestring_queue", //must be the same with RPC Server Queue Name
                                basicProperties: basicProperties,
                                body: inputBytes
                            );


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
            Send(args == null || (args != null && args.Length == 0) ? "Hello World" : args.First());

        }
    }
}
