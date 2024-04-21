using System.IO;
using System.Net.Sockets;
using System.Text;


namespace Client.Connection
{
    public class PacketReader : BinaryReader
    {
        private NetworkStream ns;

        public PacketReader(NetworkStream ns) : base(ns)
        {
            this.ns = ns;
        }
        public byte ReadOpCode()
        {
            byte opcode = ReadByte();

            return opcode;
        }
        public string ReadMessage()
        {

            byte[] msgBuff;
            var length = ReadInt32();
            msgBuff = new byte[length];
            ns.Read(msgBuff, 0, length);
            var msg = Encoding.ASCII.GetString(msgBuff);
            return msg;
        }

    }
}