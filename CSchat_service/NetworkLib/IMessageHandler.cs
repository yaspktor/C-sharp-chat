using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkLib
{
    public interface IMessageHandler
    {
        void HandleMessage(string message, string color);
    }
}
