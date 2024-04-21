using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public  class Manager
    {



        public static void BroadcastConnection(List<Client> users)
        {
            //każdemu uzytkownikowi wysyła informacje o wszystkich podłączonych użytkownikach
            foreach (var user in users)
            {
                foreach (var usr in users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteString(usr.Username);
                    broadcastPacket.WriteString(usr.Id.ToString());
                    //broadcastPacket.WriteString(usr.UsernameColor.ToString());
                    //user a nie usr bo wysylamy teraz do usera informacje o innych userach
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                    Console.WriteLine($"Sending packet to {user.Username}");
                }
            }

           // BroadcastMessage(users, $"[{disconnectUser.Username}] Disconnected!");

        }

        public static void BroadcastDisconnect(List<Client> users, string id)
        {

            var disconnectUser = users.Where(x => x.Id.ToString() == id).FirstOrDefault();



            users.Remove(disconnectUser);
            foreach (var user in users)
            {
                var discPacket = new PacketBuilder();
                discPacket.WriteOpCode(5);
                discPacket.WriteString(disconnectUser.Id.ToString());
                user.ClientSocket.Client.Send(discPacket.GetPacketBytes());

            }

            BroadcastMessage(users, $"[{disconnectUser.Username}] Disconnected!");

        }

        public static void BroadcastMessage(List<Client> users, string message, string color = "#ffffff")
        {

            foreach (var user in users)
            {
                var msgPacket = new PacketBuilder();
                msgPacket.WriteOpCode(10);
                msgPacket.WriteString(message);
                msgPacket.WriteString(color);
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());

            }
        }
    }
}
