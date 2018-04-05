using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SystemProgramming_Exam.Logic;

using Newtonsoft.Json;

namespace SystemProgramming_Exam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private RabbitMqMiddlewareBusService middleware = new RabbitMqMiddlewareBusService();
        User u = new User();


        public MainWindow()
        {
            InitializeComponent();

            //DispatcherTimer t = new DispatcherTimer();
            //t.Interval = TimeSpan.FromSeconds(5);
            //t.Tick += (a, b) =>
            //{

            //};
        }

        private int GeneRateRandomNumber()
        {
            Random rnd = new Random();
            return rnd.Next(1, 5);
        }


        public static void SayHello()
        {
            MessageBox.Show("Oh My Gad it is Working");
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            CurrentRandomNumberTextBlock.Text = null;
            try
            {
                PartnerRandomNumberTextBlock.Text += staticPartnterTextBlockText;
                int randNumber = GeneRateRandomNumber();
                CurrentRandomNumberTextBlock.Text += randNumber;
                if (!string.IsNullOrEmpty(CurrentRandomNumberTextBlock.Text))
                {
                    u.UserLuckyNumber = (CurrentRandomNumberTextBlock.Text);
                    u.UserNumber = CurrentNumberTextBlock.Text;
                    Process.Start("Consumer.exe", u.UserLuckyNumber.ToString());

                }
                else
                {
                    MessageBox.Show("Your random Number Is Empty!");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void SeachSomeBody(object sender, RoutedEventArgs e)
        {
            u.UserLuckyNumber = (CurrentRandomNumberTextBlock.Text);
            u.UserNumber = CurrentNumberTextBlock.Text;
            middleware.PublishMessage(u, u.UserLuckyNumber);
        }

        public static string staticPartnterTextBlockText;
        public static void SetOtherNumber(string number)
        {
            MessageBox.Show($"Number is - {number}");
            staticPartnterTextBlockText += number;
        }
    }
    public class User
    {
        public string UserLuckyNumber { get; set; }
        public string UserNumber { get; set; }

    }
}
