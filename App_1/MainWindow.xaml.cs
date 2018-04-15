using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using App_1.Logic;
using Exception = System.Exception;
using App_ClassLibrary;
using RabbitMQ.Client.Impl;

namespace App_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RabbitMqMiddlewareBusService _rabbitMqQ1= new RabbitMqMiddlewareBusService();
        private List<string>usernames =  new List<string>{"Egor","Nastya","Misha","Natasha","Vasya","Marina","Petya","Aleksandra"};
        private List<string> usernumbers = new List<string>{"+77051648233","+77026112508","+77036456666","+77044175423" ,"+77041245784","+77412345748","+77156354124","+77785476321"};
        //Giud Приложения
        //string guid = Marshal.GetTypeLibGuidForAssembly(Assembly.GetExecutingAssembly()).ToString();
        private readonly Random _rand = new Random();   
        public MainWindow()
        {
            InitializeComponent();
            UserNameTextBox.Text = usernames.ElementAt(_rand.Next(0, 6));
            UserPhoneNumberTextBox.Text = usernumbers.ElementAt(_rand.Next(0, 6));
        }
        private void FindCoupleButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    UserNameTextBox.Text = usernames.RandomItem();
                    UserPhoneNumberTextBox.Text = usernumbers.RandomItem();
                    usernames.RemoveAt(usernames.IndexOf(UserNameTextBox.Text));
                    usernumbers.RemoveAt(usernumbers.IndexOf(UserPhoneNumberTextBox.Text));

                    _rabbitMqQ1.PublishMessage(new User
                    {
                        AppId = (Process.GetCurrentProcess().Id).ToString(),
                        UserName = UserNameTextBox.Text,
                        UserNumber = UserPhoneNumberTextBox.Text,
                        UserRandomNumber = _rand.Next(0, 7)
                    }, "Q1");
                }
            }
            catch (Exception ex)
            {
                LogsTextBlock.Text += ex;
            }
        }
        private void UserNameTextBox_OnGotFocus(object sender, RoutedEventArgs e) { UserNameTextBox.BorderBrush = new SolidColorBrush(Colors.Blue); }
        private void UserPhoneNumberTextBox_OnGotFocus(object sender, RoutedEventArgs e) { UserPhoneNumberTextBox.BorderBrush = new SolidColorBrush(Colors.Blue); }
    }
}
