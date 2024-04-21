using Client;
using Client.Connection;
using Client.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.ServiceProcess;
using System.Configuration;

namespace CSchat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private object _lock = new object();

        private Server server;
        private ServiceController service;
        private ObservableCollection<UserModel> Users;
        private ObservableCollection<MessageModel> Messages;

        private string message;
        private string username;
        private int is_connected = 0;
        private string IpAddress = ConfigurationManager.AppSettings["IPAddress"];


        public MainWindow()
        {
            InitializeComponent();
            // wystartowanie uslugi servera jesli nie jest uruchomiona
            try
            {
                string serviceName = ConfigurationManager.AppSettings["ServiceName"];
                if (!string.IsNullOrEmpty(serviceName))
                {
                    service = new ServiceController(serviceName);
                }
                else
                {
                    service = new ServiceController("Chat");
                }

                if (service.Status == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);

                }
                Update_Label();
                ip_textbox.Text = IpAddress;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Błąd operacji na usłudze: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.ServiceProcess.TimeoutException ex)
            {
                MessageBox.Show($"Limit czasu operacji na usłudze: {ex.Message}", "Błąd limitu czasu", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Nieoczekiwany błąd: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void Update_Label()
        {
            if (service.Status == ServiceControllerStatus.Running)
            {
                status_label.Content = "Status serwera: Running";
            }
            else
            {
                status_label.Content = "Status serwera: Stopped";
            }
        }

        private void Connect_button(object sender, RoutedEventArgs e)
        {
            //rzutowanie na button
            Button button = (Button)sender;

            if (is_connected == 0)
            {
                // przyppisanie username do zmiennej
                is_connected = 1;
                username = username_textbox.Text;

                server = new Server();
                Users = new ObservableCollection<UserModel>();
                Messages = new ObservableCollection<MessageModel>();
                               
                server.connectedEvent += UserConnected;
                server.msgReceivedEvent += MessageReceived;
                server.userDisconnectEvent += UserDisconnected;

                try
                {
                    server.ConnectToServer(username);
                    text_msg.IsEnabled = true;
                    //wylaczenie przycisku po polaczeniu
                    button.IsEnabled = false;

                    //spr czy wlaczyc przycisk 
                    if (text_msg.Text.Length > 0)
                    {
                        bnt_send.IsEnabled = true;
                    }

                    button.Content = "Connected";
                    Username_label.Content = username;
                    //zmiana koloru labela na zielony  "#3bff6f";
                    Status_border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3bff6f"));
                    Status_label.Content = "Connected";

                }
                catch (InvalidOperationException ex)
                {
                    // wywolanie okna z informacja o bledzie
                    MessageBox.Show(ex.Message, "Informacja o połączeniu", MessageBoxButton.OK, MessageBoxImage.Information);
                    is_connected = 0;
                }
                
               
            }
            else
            {
                MessageBox.Show($"Already connected with username: {username}");
                button.IsEnabled = false;
            }

        }


        private void UserConnected()
        {

            //stworzenie nowego modelu uzytkownika otrzymaniu odpowiedzi od serwera 
            var user = new UserModel
            {
                Username = server.packetReader.ReadMessage(),
                Id = server.packetReader.ReadMessage(),

            };

            //jesli w kolekcji nie ma jeszcze tego usera
            if (!Users.Any(x => x.Id == user.Id)) // x to pojedynczy element kolekcji i sprawdzamy czy rowna sie userowi

            {
                //dispatcher bo connected event jest uruchomiony w innym wątku 
                Dispatcher.Invoke(() =>
                {
                    //dodanie go do listy aktywnych userow
                    Users.Add(user);
                    //dodanie do ListView nowego usera
                    Usernames_list.Items.Add(user);
                    //msg_text.Items.Add(messageModel);
                });
            }

        }


        private void UserDisconnected()
        {
            var id = server.packetReader.ReadMessage();
            var user = Users.Where(x => x.Id == id).FirstOrDefault();

            if (user != null)
            {
                Dispatcher.Invoke(() =>
                {
                    Users.Remove(user);
                    Usernames_list.Items.Remove(user);

                    //jesli to ostatni user to wylaczamy serwer
                    if (Users.Count == 0)
                    {
                        service.Stop();
                        service.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                });
            }
        }

        private void MessageReceived()
        {

            //poniewaz serwer wysyla wiadomosc w formacie [time]:[id]:[username]:[message] to musimy to rozdzielic

            var msg = server.packetReader.ReadMessage();
            var color = server.packetReader.ReadMessage();
            var regex = new Regex(@"\[(.*?)\]:\[(.*?)\]:\[(.*?)\]");
            var match = regex.Match(msg);


            if (match.Success)
            {
                var messageModel = new MessageModel
                {
                    Time = match.Groups[1].Value,
                    Username = match.Groups[2].Value,
                    Message = match.Groups[3].Value,
                    Type = true,
                    UsernameColor = color
                };

                Dispatcher.Invoke(() =>
                {

                    Messages.Add(messageModel);
                    msg_text.Items.Add(messageModel);
                });
            }
            else
            {
                var messageModel = new MessageModel
                {
                    Message = msg,
                    Type = false

                };
                Dispatcher.Invoke(() =>
                {

                    Messages.Add(messageModel);
                    msg_text.Items.Add(messageModel);
                });
            }


        }
        private void username_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {

            //rzutowanie na textbox
            TextBox textbox = (TextBox)sender;
            //sprawdzenie czy textbox nie jest pusty i czy len > 3
            if (username_textbox.Text.Length > 3)
            {
                //wlaczenie przycisku
                bnt_connect.IsEnabled = true;
            }
            else
            {
                //wylaczenie przycisku
                bnt_connect.IsEnabled = false;
            }

        }

        private void bnt_send_Click(object sender, RoutedEventArgs e)
        {

            if (text_msg.Text.Length > 0 && is_connected == 1)
            {
                message = text_msg.Text;
                server.SendMessageToServer(message);
                text_msg.Text = "@Message";
            }

        }

        private void text_msg_TextChanged(object sender, TextChangedEventArgs e)
        {
            //jesli nie polaczany to wylacz textbox
            TextBox textbox = (TextBox)sender;

            if (is_connected == 0)
            {
                textbox.IsEnabled = false;
            }
            else
            {
                textbox.IsEnabled = true;
            }


        }


        private void Border_mouse_down(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                DragMove();

            }
        }

        private void Button_minimize(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void maximize_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {

                Application.Current.MainWindow.WindowState = WindowState.Maximized;

            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }

        }

        private void close_bnt_Click(object sender, RoutedEventArgs e)
        {
            // wylacz usluge jesli ostatni user sie rozlaczy
            if (Users.Count == 1)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped);
            }
            
            Application.Current.Shutdown();
        }

        private void myTextBox_KeyDown(object sender, KeyEventArgs e)
        {

            //czy polaczony

            if (is_connected == 1)
            {
                //usuniecie @Message z textboxa
                if (text_msg.Text == "@Message")
                {
                    text_msg.Text = "";
                }

            }


            if (text_msg.Text.Length > 0 && is_connected == 1)
            {
                if (e.Key == Key.Enter)
                {

                    message = text_msg.Text;
                    server.SendMessageToServer(message);
                    text_msg.Text = "@Message";
                }
            }


        }

        private void pipe(object sender, MouseButtonEventArgs e)
        {
            //dodanie migotającego | na koncu tekstu asynchronicznie
            if (is_connected == 1)
            {
                text_msg.Text = "|";
            }
        }

        private void Username_KeyDown(object sender, KeyEventArgs e)
        {

            //czy polaczony

            if (is_connected == 0)
            {
                //usuniecie @Message z textboxa
                if (username_textbox.Text == "@Username")
                {
                    username_textbox.Text = "";
                }
            }


            if (username_textbox.Text.Length > 2 && is_connected == 0)
            {

                bnt_connect.IsEnabled = true;
            }
        }

        private void Ip_KeyDown(object sender, KeyEventArgs e)
        {
            if (is_connected == 0)
            {
                //usuniecie @Message z textboxa
                // i wstawienie ip z congifu 
                if (ip_textbox.Text == IpAddress)
                {
                    ip_textbox.Text = "";
                }
            }

            //jesli cos wpisane to wlacz przycisk
            if (ip_textbox.Text.Length > 6 && is_connected == 0)
            {
                bnt_ipt.IsEnabled = true;
            }
        }

        private void change_button(object sender, RoutedEventArgs e)
        {

            //rzutowanie na button
            Button button = (Button)sender;

            //jesli polaczony to wylacz przycisk
            if (is_connected == 1)
            {
                button.IsEnabled = false;
            }
            else
            {
                button.IsEnabled = true;
            }

            //wpisanie do pliku konfiguracyjnego nowej wartosci ip
            if (ip_textbox.Text.Length > 6)
            {
                // plik konfiguracyjny
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                // zmień wartość
                config.AppSettings.Settings["IPAddress"].Value = ip_textbox.Text;

                config.Save(ConfigurationSaveMode.Modified);

                // refresh
                ConfigurationManager.RefreshSection("appSettings");

                //MessageBox.Show("Wymagany jest restart aplikacji");

            }

        }
    }
}
