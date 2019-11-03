using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace MqManualReceiverV1
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
                using (IConnection connection = connectionFactory.CreateConnection())
                {
                    using (IModel channel = connection.CreateModel())
                    {
                        //It will be created only if queue doesn't exist
                        channel.QueueDeclare(
                                queue: "queue_manual",
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
                            Console.WriteLine($"\nProcessing message `{message}`....");
                            Thread.Sleep(5000);
                            Console.WriteLine($"Message: `{message}` was processed");
                            channel.BasicAck(
                                    deliveryTag: payload.DeliveryTag,
                                    multiple: false
                                );
                        };
                        channel.BasicConsume(
                                queue: "queue_manual",
                                autoAck: false,
                                consumer: consumer
                            );
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
            Listen();

        }
    }
}
