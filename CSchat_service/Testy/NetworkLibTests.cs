using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testy
{
    [TestClass]
    public class NetworkLibTests
    {
       

        [TestMethod]
        public void Client_Constructor_TcpClientNull_ShouldThrowArgumentNullException()
        {

            Assert.ThrowsException<ArgumentNullException>(() => new NetworkLib.Client(null, null, null), "Powinien zostac wyrzucony wyjątek");
        }

        [TestMethod]
        public void WriteOpCode_ShouldWriteToStream()
        {
            // Arrange
            var packetBuilder = new PacketBuilder();
            byte opcode = 10; // przykładowy opcode

            // Act
            packetBuilder.WriteOpCode(opcode);
            byte[] result = packetBuilder.GetPacketBytes();

            // Assert
            Assert.AreEqual(opcode, result[0], "Pierwszy bajt pakietu powinien byc to opcode");
        }

        [TestMethod]
        public void WriteString_ShouldWriteToStream()
        {
            // Arrange
            var packetBuilder = new PacketBuilder();
            byte opcode = 10; // przykładowy opcode
            string str = "Test message";

            // Act
            packetBuilder.WriteOpCode(opcode);
            packetBuilder.WriteString(str);
            byte[] result = packetBuilder.GetPacketBytes();

            // Assert
            int length = BitConverter.ToInt32(result, 1);
            Assert.AreEqual(str.Length, length, "Bajty 1-4 powinny zawierac dlugosc wiadomosci");

            string msg = Encoding.ASCII.GetString(result, 5, length);
            Assert.AreEqual(str, msg, "Reszta pakietu powinna być wiadomoscia");
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
