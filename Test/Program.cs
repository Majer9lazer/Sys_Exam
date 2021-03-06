﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using App_ClassLibrary;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
            RunWorkerProcessForSmss("Q2");
        }

        private static RabbitMqMiddlewareBusService _pubMessage = new RabbitMqMiddlewareBusService();
        public static IConnectionFactory ConnectionFactory;
        public static void RunWorkerProcessForSmss(string queueName = "smss_to_send")
        {
            
            using (var connection = ConnectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                Console.WriteLine((" [*] Opened Channel\n"));

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine($" [*] Waiting for messages from queue {queueName}\n");

                var consumer = new EventingBasicConsumer(channel);

                try
                {

                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        var messageDeserialized = JsonConvert.DeserializeObject<User[]>(message);


                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        using (StreamWriter sw = new StreamWriter("UserCouple.txt", false))
                        {

                            sw.Write(JsonConvert.SerializeObject(messageDeserialized));
                        }

                        if (messageDeserialized.Length == 2)
                        {
                            //Console.ForegroundColor = ConsoleColor.Green;

                            Console.WriteLine($"We have a couple! - {messageDeserialized[0].UserName} and {messageDeserialized[1].UserName} , RandomNumber = {messageDeserialized[0].UserRandomNumber} , 2Random = {messageDeserialized[1].UserRandomNumber}");
                            _pubMessage.PublishMessage(messageDeserialized,"Q3");
                            SystemSounds.Beep.Play();
                        }

                    };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    channel.BasicConsume(queue: queueName,
                        autoAck: false,
                        consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
        public class RabbitMqMiddlewareBusService
        {
            private readonly IConnectionFactory _connectionFactory;
            public void PublishMessage<T>(T message, string queueName) where T : class
            {
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                        routingKey: queueName,
                        basicProperties: properties,
                        body: body);
                }
            }
            public RabbitMqMiddlewareBusService()
            {
                _connectionFactory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/"
                };
            }
        }
    }
}
