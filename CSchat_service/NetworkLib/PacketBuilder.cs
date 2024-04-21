using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public class PacketBuilder
    {
        MemoryStream ms; // strumien bajtow 

        public PacketBuilder()
        {
            ms = new MemoryStream();
        }

        // metoda do zapisu bajtow do strumienia opcode - kod operacji sluzy jako informacja jak interpretowac dane
        // 0 - polaczenie z serwerem i username
        // 1 - broadcast


        public void WriteOpCode(byte opcode)
        {
            //byte co wystarczy na 256 roznych kodow operacji
            ms.WriteByte(opcode); // początek strumienia bajtów to kod operacji
        }


        public void WriteString(string str)
        {
            var msgBytes = Encoding.ASCII.GetBytes(str);
            var msgLength = msgBytes.Length;
            var lengthBytes = BitConverter.GetBytes(msgLength);

            // Zapisz długość wiadomości
            ms.Write(lengthBytes, 0, lengthBytes.Length);

            // Zapisz wiadomość
            ms.Write(msgBytes, 0, msgBytes.Length);
        }




        public byte[] GetPacketBytes()
        {
            return ms.ToArray();
        }

        /*        
         ---------------------------------
         |      |         |              |
         |      |         |              |
         | op   | dlugosc |   wiadomosc  |
         | code |   4b    |              |
         | 1b   |         |              |
         |      |         |              |
         ---------------------------------           
         */
    }
}
