using Client.MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Configuration;

namespace Client.Connection
{
    public class Server
    {

        TcpClient client; 
        public PacketReader packetReader;
        public event Action connectedEvent; 
        public event Action msgReceivedEvent; 
        public event Action userDisconnectEvent;
        public ObservableCollection<UserModel> Users { get; set; }
        private string IpAddress = ConfigurationManager.AppSettings["IPAddress"];
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);


        public Server()
        {
            client = new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            try
            {

                if (!client.Connected)
                {

                    try { 
                   
                    client.Connect(IpAddress, port);
                    }
                    catch (Exception ex)
                    {
                        
                        throw new InvalidOperationException($"Server is not available: {ex.Message}", ex);
                    }
                    //do odczytywania wiadomosci od serwera
                    packetReader = new PacketReader(client.GetStream());

                    if (!string.IsNullOrEmpty(username))
                    {
                        //nowa instancja packet builder, musimy przygotowac pakiet do wyslania
                        var packetToSend = new PacketBuilder();
                        packetToSend.WriteOpCode(0); //0 - connect opcode 
                        packetToSend.WriteString(username);
                        // po przygotowaniu pakietu wysylamy go do serwera jako tablica bajtow
                        client.Client.Send(packetToSend.GetPacketBytes());
                        Console.WriteLine("Connected");

                    }
                    //funkcja odpowiedzialna za odczytywanie odpowiedzi od  serwera 
                    ReadPackets();

                }
                else
                {
                    Console.WriteLine("Already connected");
                }
            }

            catch (SocketException ex)
            {
                // Rzuć nowy wyjątek z bardziej zrozumiałym komunikatem
                throw new InvalidOperationException($"Nie można się połączyć z serwerem: {ex.Message}", ex);
            }

        }

        private void ReadPackets()
        {
            // w innym wątku aby aplikacja sie nie zacinała 
            Task.Run(() =>
            {

                while (true)
                {
                    //patrzymy jaki otrzymalismy opcode, nie sprawdzamy opcone 0 bo to robimy w innym miejscu i tylko klient wysyla opcode == 0
                    var opcode = packetReader.ReadOpCode();
                    switch (opcode)
                    {

                        case 1:
                            //czyli broadcast
                            Console.WriteLine("[Client]: Packet received");
                            connectedEvent?.Invoke(); //uruchomienie eventu 


                            break;

                        case 5:
                            //czyli disconnect

                            userDisconnectEvent?.Invoke(); //uruchomienie eventu 


                            break;
                        case 10:
                            //czyli message 
                            
                            msgReceivedEvent?.Invoke(); //uruchomienie eventu 


                            break;



                        default:
                            Console.WriteLine("tets");
                            break;

                    }
                }

            });

        }


        public void SendMessageToServer(string msg)
        { 
            //jesli nie jestesmy polaczeni to nie mozemy wyslac wiadomosci
            if (!client.Connected)
            {
                throw new InvalidOperationException("You are not connected to the server");
            }
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(10); // 10 - wiadomosc
            messagePacket.WriteString(msg);
            client.Client.Send(messagePacket.GetPacketBytes());
            
        }


    }
}