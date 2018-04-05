using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemProgramming_Exam;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
namespace Consumer
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                if (args.Any())
                {

                    GetSmsFromRabbitMq q = new GetSmsFromRabbitMq();
                    q.RunWorkerProcessForSmss(args[0]);
                }

                else
                {
                    Console.WriteLine("В аргументы ничего не пришло");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
                Console.ReadLine();
            }
        }
    }

    public class GetSmsFromRabbitMq
    {
        public static IConnectionFactory _connectionFactory;



        public void RunWorkerProcessForSmss(string queueName = "smss_to_send")
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                Console.WriteLine(" [*] Opened Channel");

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine($" [*] Waiting for messages from queue {queueName}");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    
                  
                    User messageDeserialized = JsonConvert.DeserializeObject<User>(message);
                    using (StreamWriter sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "Hello.txt"))
                    {
                        sw.Write(message);
                    MainWindow.SetOtherNumber(messageDeserialized.UserNumber);
                    }
                    Console.WriteLine($" [x] Deserialized object {messageDeserialized}");

                    Console.WriteLine($" [x] Received {message}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" [x] Operation Completed {message}");
                    Console.ForegroundColor = ConsoleColor.Green;

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        

        public GetSmsFromRabbitMq()
        {
            _connectionFactory = new ConnectionFactory()
            {
                HostName = "192.168.111.199",
                UserName = "shag",
                Password = "shag",
                VirtualHost = "/"
            };
        }
    }
}
