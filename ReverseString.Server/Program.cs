using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Linq;

namespace ReverseString.Server
{
    class Program
    {
        private static string ReverseString(string input) => new string(input.Reverse().ToArray());

        private static void Listen()
        {
            

            IConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            using(IConnection connection = connectionFactory.CreateConnection())
            {
                using(IModel channel = connection.CreateModel())
                {
                    channel.QueueDeclare(
                            queue: "reversestring_queue",
                            durable: false,
                            exclusive: false,
                            autoDelete: false,
                            arguments: null
                        );
                    channel.BasicQos(
                            prefetchSize: 0,
                            prefetchCount: 1,
                            global: false
                        );
                    Console.WriteLine("[*] Waiting for request...");
                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (sender, payload) =>
                    {
                        string result = "";
                        string input = Encoding.UTF8.GetString(payload.Body);
                        IBasicProperties requestProps = payload.BasicProperties;
                        IBasicProperties responseProps = channel.CreateBasicProperties();
                        responseProps.CorrelationId = requestProps.CorrelationId;

                        try
                        {
                            result = ReverseString(input);
                            Console.WriteLine("[#] Incoming request.");
                            Console.WriteLine($"[*] `{input}` -> `{result}`");
                        }
                        catch (Exception ex)
                        {
                            result = "";
                            Console.WriteLine(ex);

                        }
                        finally
                        {
                            byte[] resultBytes = Encoding.UTF8.GetBytes(result);
                            channel.BasicPublish(
                                    exchange: "",
                                    routingKey: requestProps.ReplyTo,
                                    basicProperties: responseProps,
                                    body: resultBytes
                                );
                            channel.BasicAck(deliveryTag: payload.DeliveryTag, multiple: false);
                        }
                    };
                    channel.BasicConsume(
                            queue: "reversestring_queue",
                            autoAck: false,
                            consumer: consumer
                        );
                    Console.WriteLine("[!] Press [ENTER] to quit");
                    Console.ReadLine();
                }
            }

        }

        static void Main(string[] args)
        {
            Listen();
        }
    }
}
