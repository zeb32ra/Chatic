using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Timers;

namespace chat_chat
{
    /// <summary>
    /// Логика взаимодействия для windwow.xaml
    /// </summary>
    public partial class windwow : Window
    {
        private Socket socket;
        private Socket socket_server;
        private List<Socket> clients = new List<Socket>();
        List<string> users = new List<string>();
        MainWindow window = new MainWindow();
        string name;
        string ip;
        string user;
        string message_name;
        int c = 0;
        public windwow(string name_, string ip_)
        {
            name = name_;
            ip = ip_;
            InitializeComponent();
            //list_user.ItemsSource= users;
            user = "/username" + " " + name;
            if (MainWindow.IsServer == true)
            {

                IPEndPoint ip_point = new IPEndPoint(IPAddress.Any, 8888);
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(ip_point);
                socket.Listen(1000);
                ListenClients();

                // тут надо сделать еще один сокет для клиента, чтобы у тебя был и сервак и клиент одновременно
                user = "/username" + " " + "ADMINISTRATOR";

                socket_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket_server.ConnectAsync(ip, 8888); //ip - 127.0.0.1
                RecieveMessage_Serv(socket_server);
                SendMessage(socket_server, user);
            }
            else
            {
                log_chat.Visibility = Visibility.Hidden;
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ConnectAsync(ip, 8888); //ip - 127.0.0.1

                
                RecieveMessage(socket);
                SendMessage(socket, user);
            }

            
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            SendMessage(socket, "/diskonect" + " " + name);
            
            this.Close();
        }




        private async Task ListenClients()
        {
            while (true)
            {
                var client = await socket.AcceptAsync();
                clients.Add(client);
                
                list_user_log.Items.Add(client.RemoteEndPoint);
                
                RecieveMessage(client);
                
            }

        }

        private async Task RecieveMessage(Socket client)
        {
            /*list_user.Items.Clear();*/
            
            while (true)
            {
               
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes);

                //MessageBox.Show(message.TrimStart());
                
                if (message.StartsWith("/diskonect"))
                {
                    string[] a = message.Split(" ");
                    //MessageBox.Show(a[1]);
                    foreach (string i in list_user.Items)
                    {
                        string[] b = i.Split("\0");
                        //MessageBox.Show((b[0] + " " + b[1]));

                        if ((b[0] + " " + b[1]) == "/username " + a[1])
                        {
                            users.Remove(i);
                        }
                        //users.Remove("/username " + a[1]);
                    }
                    list_user.ItemsSource = users;
                    //MessageBox.Show("/username " + a[1] + "  777");
                }

                if (MainWindow.IsServer == true)
                {
                    
                    foreach (var item in clients)
                    {
                        if (message.StartsWith("/username"))
                        {
                            //SendMessage(item, message);
                            users.Add(message);
                            //list_user.Items.Add(message);
                            list_message.Items.Add(message);
                            list_user.Items.Add(message);
                            foreach (var items in list_user.Items)
                            {
                                if (message != item.ToString())
                                {
                                    //list_user.Items.Add(message);
                                    SendMessage(item, items.ToString());
                                }
                            }
                        }
                        else
                        {
                            
                            SendMessage(item, message);
                        }
                    }
                    

                }
                else
                {
                    if (message.StartsWith("/username"))
                    {
                        
                        users.Add(message);
                        list_user.Items.Add(message);
                        /*foreach (var item in users)
                        {
                            list_user.Items.Add(item);
                        }*/
                        //users.Clear();
                    }
                    else
                    {
                        string[] ms = message.Split("{");
                        list_message.Items.Add($"[Весточка от {ms[0]} - {ms[1]}]: {ms[2]}");
                    }

                    

                    //list_message.Items.Add(message);
                }


                
            }

        }

        private async Task SendMessage(Socket client, string message)
        {

            byte[] bytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(bytes, SocketFlags.None);
        }


        private async Task RecieveMessage_Serv(Socket client)
        {
            while (true)
            {
                
                byte[] bytes = new byte[1024];
                await client.ReceiveAsync(bytes, SocketFlags.None);
                string message = Encoding.UTF8.GetString(bytes);
                //list_message.Items.Add($"[Весточка от {name}]: {message}");
                if (message.StartsWith("/diskonect"))
                {
                    string[] a = message.Split(" ");
                    //MessageBox.Show(a[1]);
                    foreach (var i in list_user.Items)
                    {

                        list_user.Items.Remove(list_user.Items.IndexOf("/username " + a[1]));
                    }
                }

                if (message.StartsWith("/username"))
                {
                    //users.Add(message);
                    //list_user.Items.Add(message);
                    list_user.Items.Add(message);
                }
                else
                {
                    string[] ms = message.Split("{");
                    list_message.Items.Add($"[Весточка от {ms[0]} - {ms[1]}]: {ms[2]}");
                }
            }

        }





        private void message_Click(object sender, RoutedEventArgs e)
        {
            string time = DateTime.Now.ToString();
            string mes;
            if (MainWindow.IsServer == true)
            {
                mes = "ADMINISTRATOR" + "{" + time.ToString() + "{" + vestohka.Text;
                SendMessage(socket_server, mes);
            }
            else
            {
                mes = name + "{" + time.ToString() + "{" + vestohka.Text;
                SendMessage(socket, mes);
            }
        }

        private void log_chat_Click(object sender, RoutedEventArgs e)
        {
            if (c == 0)
            {
                list_user.Visibility = Visibility.Hidden;
                list_user_log.Visibility = Visibility.Visible;
                c = 1;
            }
            else if (c == 1)
            {
                list_user.Visibility = Visibility.Visible;
                list_user_log.Visibility = Visibility.Hidden;
                c = 0;
            }
        }
    }
}
