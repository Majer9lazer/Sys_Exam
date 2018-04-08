using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using App_ClassLibrary;
using Consumer.RabbitMqPublishMessage;
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
                GetSmsFromRabbitMq q = new GetSmsFromRabbitMq();
                q.RunWorkerProcessForSmss("Q1");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally { Console.ReadLine(); }
        }
    }

    public class GetSmsFromRabbitMq
    {
        public static IConnectionFactory ConnectionFactory;
        RabbitMqMiddlewareBusService _toPublishMessageQ2 = new RabbitMqMiddlewareBusService();
        private readonly List<User> _users = new List<User>();
        public void RunWorkerProcessForSmss(string queueName = "smss_to_send")
        {

            using (var connection = ConnectionFactory.CreateConnection())
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

                    var messageDeserialized = JsonConvert.DeserializeObject<User>(message);

                    if (_users.Count <= 5)
                    {
                        _users.Add(messageDeserialized);
                    }
                    if (_users.Count >= 6)
                    {

                        using (StreamWriter sw = new StreamWriter("UsersInformation.txt", false))
                        {
                            var couple = GetCouple(_users);
                            if (couple != null)
                            {
                                sw.Write(JsonConvert.SerializeObject(couple));
                                _toPublishMessageQ2.PublishMessage(couple, "Q2");
                            }
                            else
                            {
                                Console.WriteLine("We didn't find couple");
                            }

                        }
                    }


                    Console.WriteLine($" [x] Deserialized object {messageDeserialized}");
                    Console.WriteLine($" [x] Received {message}");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($" [x] Operation Completed {message}");
                    Console.ForegroundColor = ConsoleColor.Green;

                    // ReSharper disable once AccessToDisposedClosure
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: queueName,
                    autoAck: false,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private User[] GetCouple(List<User> users)
        {
            var firstTwoElements = users.Take(users.Count / 2);
            var lastTwoElements = users.Skip(users.Count / 2).Take(users.Count / 2);
            User firstCouple = firstTwoElements.Intersect(lastTwoElements, new UserComparer()).ToList().FirstOrDefault();
            users = users.OrderBy(o => o.UserRandomNumber).ToList();
            for (int i = 0; i < users.Count; i++)
            {
                for (int j = i + 1; j < users.Count; j++)
                {
                    if (users[i].UserRandomNumber == users[j].UserRandomNumber)
                    {
                        return new[] {users[i], users[j]};
                    }
                }
            }


            return null;
        }
        public GetSmsFromRabbitMq()
        {
            ConnectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
        }
    }
}
