using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace AppServer
{
    public partial class MainWindow : Window
    {
        private TcpListener listener;
        private List<TcpClient> clients = new List<TcpClient>();

        public MainWindow()
        {
            InitializeComponent();
            StartServer();
        }

        private async void StartServer()
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("192.168.1.69"), 27001);
                listener.Start();
                Console.WriteLine("Server started. Listening for connections...");

                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    clients.Add(client);
                    HandleClient(client);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string name = await reader.ReadLineAsync();
                    Application.Current.Dispatcher.Invoke(() => clientListBox.Items.Add(name));

                    while (true)
                    {
                        string message = await reader.ReadLineAsync();
                        BroadcastMessage(name, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client connection: {ex.Message}");
            }
        }

        private async void BroadcastMessage(string senderName, string message)
        {
            foreach (TcpClient client in clients)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream);
                    await writer.WriteLineAsync($"{senderName}: {message}");
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting message: {ex.Message}");
                }
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string message = messageTxt.Text;

                string selectedClient = clientListBox.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(selectedClient))
                {
                    MessageBox.Show("Please select a client to send the message.");
                    return;
                }

                BroadcastMessage(selectedClient, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}
