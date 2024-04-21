using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class Client
    {
        public string Username { get; set; }
        public Guid Id { get; set; } //Guid słuzacy do wygenerowania unikalnego id dla kazdego klienta
        public string UsernameColor { get; set; }
        public TcpClient ClientSocket { get; set; }
        PacketReader packetReader { get; set; }

        private IMessageHandler messageHandler;
        private IDisconnectionHandler disconnectionHandler;

        public Client(TcpClient client, IMessageHandler messageHandler, IDisconnectionHandler disconnectionHandler)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));
            if (disconnectionHandler == null) throw new ArgumentNullException(nameof(disconnectionHandler));
            this.messageHandler = messageHandler;
            this.disconnectionHandler = disconnectionHandler;

            ClientSocket = client;
            Id = Guid.NewGuid();
            // packetReader w konstruktorze potrzebuje NetworkStream, który pobieramy z TcpClient
            packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = packetReader.ReadOpCode(); //odczytujemy opcode readByte 
            //jesli opcode jest rozny od 0 to rozłaczamy 

            if (opcode != 0)
            {
                Console.WriteLine("Invalid opcode");
                ClientSocket.Close();
                return;
            }

            Username = packetReader.ReadMessage(); //odczytujemy username

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");
            //kolor uzytkownika

            string[] colors = new string[]
                {
                "#FF0000", // Red
                "#00FF00", // Green
                "#0000FF", // Blue
                "#FFFF00", // Yellow
                "#00FFFF", // Aqua
                "#FF00FF", // Magenta
                "#C0C0C0", // Silver
                "#800000", // Maroon
                "#808000", // Olive
                "#008000", // Green 
                "#800080", // Purple
                "#008080", // Teal
                "#000080", // Navy
                "#90EE90", // LightGreen
                "#D3D3D3", // LightGrey
                "#ADD8E6", // LightBlue
                "#E0FFFF", // LightCyan
                "#FFB6C1", // LightPink
                "#FAEBD7", // AntiqueWhite
                "#7FFFD4", // Aquamarine
                "#F0F8FF", // AliceBlue
                "#F5F5DC", // Beige
                "#FFE4C4", // Bisque
            
                };

            Random rand = new Random();

            UsernameColor = colors[rand.Next(colors.Length)];




            Task.Run(() => ReceiveMessage()); //uruchamiamy nowy wątek, który będzie odbierał wiadomości od klienta
        }

        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    var opcode = packetReader.ReadOpCode();
                    switch (opcode)
                    {
                        case 10:
                            var msg = packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message received: {msg}");
                            //wysłanie wiadomosci do wszystkich userow 
                            messageHandler?.HandleMessage($"[{DateTime.Now}]:[{Username}]:[{msg}]", UsernameColor);


                            break;

                        default:
                            break;

                    }

                }
                catch (Exception)
                {
                    Console.WriteLine($"[{Username}]: Disconnected");
                    disconnectionHandler?.HandleDisconnection(Id.ToString());
                    ClientSocket.Close();
                }
            }
        }

    }
}
