using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using App_ClassLibrary;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            ConnectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
            InitializeComponent();
            try
            {
                RunWorkerProcessForSmss("Q2");
            }
            catch (Exception e)
            {
                ErrorOrSuccesTextBlock.Text += e;
            }
        }
        public static IConnectionFactory ConnectionFactory;
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
                Dispatcher.InvokeAsync(() => { ErrorOrSuccesTextBlock.Text += (" [*] Opened Channel\n"); });



                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                Dispatcher.InvokeAsync(() =>
                {
                    ErrorOrSuccesTextBlock.Text += ($" [*] Waiting for messages from queue {queueName}\n");
                });



                var consumer = new EventingBasicConsumer(channel);


                channel.BasicConsume(queue: queueName,
                    autoAck: true,
                    consumer: consumer);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var messageDeserialized = JsonConvert.DeserializeObject<User[]>(message);
                    Dispatcher.InvokeAsync(() =>
                    {
                        ErrorOrSuccesTextBlock.Text += "We ReceivedMessage\n";
                    });


                    using (StreamWriter sw = new StreamWriter("UserCouple.txt", false))
                    {

                        sw.Write(JsonConvert.SerializeObject(messageDeserialized));
                    }

                    if (messageDeserialized.Length == 2)
                    {
                        //Console.ForegroundColor = ConsoleColor.Green;
                        MessageBox.Show($"We have a couple! - {messageDeserialized[0].UserName} and {messageDeserialized[1].UserName} , RandomNumber = {messageDeserialized[0].UserRandomNumber} , 2Random = {messageDeserialized[1].UserRandomNumber}");
                    }
                    else
                    {
                        MessageBox.Show("We didnt't have a couple");
                    }
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };



            }

        }

        private void GetLuckyCouple(object sender, RoutedEventArgs e)
        {
            try
            {
                RunWorkerProcessForSmss("Q2");

                Dispatcher.InvokeAsync(delegate { ErrorOrSuccesTextBlock.Text += "Started Process\n"; });

                if (File.Exists("UserCouple.txt"))
                {
                    using (StreamReader sr = new StreamReader("UserCouple.txt"))
                    {
                        UserInfoListView.ItemsSource = JsonConvert.DeserializeObject<User[]>(sr.ReadToEnd()).ToList();
                    }
                }
            }
            catch (Exception ex)
            {

                ErrorOrSuccesTextBlock.Text += ex;
            }
        }
    }
}
