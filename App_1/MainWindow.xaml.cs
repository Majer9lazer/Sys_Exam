using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using App_1.Logic;
using Exception = System.Exception;
using App_ClassLibrary;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Impl;
using RabbitMqMiddlewareBusService = App_1.Logic.RabbitMqMiddlewareBusService;

namespace App_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RabbitMqMiddlewareBusService _rabbitMqQ1 = new RabbitMqMiddlewareBusService();
        private List<string> usernames = new List<string> { "Egor", "Nastya", "Misha", "Natasha", "Vasya", "Marina", "Petya", "Aleksandra" };
        private List<string> usernumbers = new List<string> { "+77051648233", "+77026112508", "+77036456666", "+77044175423", "+77041245784", "+77412345748", "+77156354124", "+77785476321" };
        //Giud Приложения
        //string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();
        private readonly Random _rand = new Random();
        public MainWindow()
        {
            InitializeComponent();
            UserNameTextBox.Text = usernames.ElementAt(_rand.Next(0, 6));
            UserPhoneNumberTextBox.Text = usernumbers.ElementAt(_rand.Next(0, 6));

            ConnectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
            new Thread(delegate ()
            {
                RunWorkerProcessForSmss("Q3");
            }).Start();

        }
        private void FindCoupleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //for (int i = 0; i < 6; i++)
                //{
                //}

                UserNameTextBox.Text = usernames.RandomItem();
                UserPhoneNumberTextBox.Text = usernumbers.RandomItem();
                usernames.RemoveAt(usernames.IndexOf(UserNameTextBox.Text));
                usernumbers.RemoveAt(usernumbers.IndexOf(UserPhoneNumberTextBox.Text));

                _rabbitMqQ1.PublishMessage(new User
                {
                    AppId = (Process.GetCurrentProcess().Id).ToString(),
                    UserName = UserNameTextBox.Text,
                    UserNumber = UserPhoneNumberTextBox.Text,
                    UserRandomNumber = _rand.Next(0, 0)
                }, "Q1");

            }
            catch (Exception ex)
            {
                LogsTextBlock.Text += ex;
            }
        }
        public static IConnectionFactory ConnectionFactory;
        public void RunWorkerProcessForSmss(string queueName = "smss_to_send")
        {

            var connection = ConnectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            //Console.WriteLine((" [*] Opened Channel\n"));

            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            //Console.WriteLine($" [*] Waiting for messages from queue {queueName}\n");

            var consumer = new EventingBasicConsumer(channel);

            try
            {

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var messageDeserialized = JsonConvert.DeserializeObject<User[]>(message);


                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);


                    if (messageDeserialized.Length == 2)
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;

                        Dispatcher.InvokeAsync(() =>
                            {
                                User currUser = messageDeserialized.FirstOrDefault(w => w.UserNumber == UserPhoneNumberTextBox.Text);
                                if (currUser != null)
                                {
                                    SystemSounds.Beep.Play();
                                    LogsTextBlock.Text += "Oh my gad  you have partner\n";
                                    switch (messageDeserialized.ToList().IndexOf(currUser))
                                    {
                                        case 0:
                                            {
                                                LogsTextBlock.Text += $"name is - {messageDeserialized[1].UserName} , number is  - {messageDeserialized[1].UserNumber}\n"; break;
                                            }
                                        case 1:
                                            {
                                                LogsTextBlock.Text += $"name is - {messageDeserialized[0].UserName} , number is  - {messageDeserialized[0].UserNumber}\n"; break;
                                            }
                                        default:
                                            {
                                                LogsTextBlock.Text += "Oh doesn't exists";
                                                break;
                                            }
                                    }



                                }
                            });
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

        private void UserNameTextBox_OnGotFocus(object sender, RoutedEventArgs e) { UserNameTextBox.BorderBrush = new SolidColorBrush(Colors.Blue); }
        private void UserPhoneNumberTextBox_OnGotFocus(object sender, RoutedEventArgs e) { UserPhoneNumberTextBox.BorderBrush = new SolidColorBrush(Colors.Blue); }
    }
}
