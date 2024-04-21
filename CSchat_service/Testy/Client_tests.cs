using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Connection;
using System.Net.Sockets;
using Moq;


namespace Testy
{
    [TestClass]
    public class Client_tests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConnectToServer_ThrowsInvalidOperationExceptionOnSocketException()
        {
           
            var server = new Server();

            server.ConnectToServer("testUsername");


        }

        [TestMethod]
        public void ConnectToServer_ThrowsInvalidOperationException()
        {

            Assert.ThrowsException<InvalidOperationException>(() => new Server().ConnectToServer("testUsername"), "Powinien zostac wyrzucony wyjątek");

        }

        [TestMethod]
        public void SendMessageToServer_NotConnected_ThrowsInvalidOperationException()
        {
            
            var server = new Server(); 
            
            Assert.ThrowsException<InvalidOperationException>(() => server.SendMessageToServer("test message"), "Powinien zostac wyrzucony wyjątek  InvalidOperationException, brak połączenia z serwerem.");
        }

        [TestMethod]
        public void WriteOpCode_ShouldAddOpCodeToPacket()
        {
           
            var packetBuilder = new PacketBuilder();
            var expectedOpCode = (byte)1;

           
            packetBuilder.WriteOpCode(expectedOpCode);
            var packetBytes = packetBuilder.GetPacketBytes();

            
            Assert.AreEqual(expectedOpCode, packetBytes[0], "OpCode powinien byc pierwszym bajtem.");
        }

        [TestMethod]
        public void WriteString_ShouldAddStringToPacketWithLength()
        {
            
            var packetBuilder = new PacketBuilder();
            var testString = "Test";
            var testOpCode = (byte)0;
            packetBuilder.WriteOpCode(testOpCode); // Dodanie OpCode, aby symulować zwykle uzycie funkcji
            
            packetBuilder.WriteString(testString);
            var packetBytes = packetBuilder.GetPacketBytes();
            var length = BitConverter.ToInt32(packetBytes, 1); // Długość jest zapisana na 4 bajtach po OpCode 
            Assert.AreEqual(testString.Length, length, "Długość wiadomości powinna być dodana do pakietu");
            var actualString = Encoding.ASCII.GetString(packetBytes, 5, length);
            Assert.AreEqual(testString, actualString, "Wiadomość powinna zostać dodana po dodaniu jej długości.");
        }

        [TestMethod]
        public void GetPacketBytes_ShouldReturnByteArrayOfPacket()
        {
            
            var packetBuilder = new PacketBuilder();
            packetBuilder.WriteOpCode((byte)0); // Dodanie OpCode
            var testString = "Hello";
            packetBuilder.WriteString(testString); // Dodanie stringa

           
            var packetBytes = packetBuilder.GetPacketBytes();

           
            Assert.IsTrue(packetBytes.Length > 0, "Pakiet powinien coś zawierać.");
            // not null
            Assert.IsNotNull(packetBytes, "Pakiet nie powinien być puste.");
        }



    }
}
