using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MVVM.Model
{
    internal class MessageModel
    {
        public string Username { get; set; }
        public string uId { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        //przechowywanie koloru wiadomosci w celu wyswietlenia jej w odpowiednim kolorze w WPF
        public string UsernameColor { get; set; }
        public bool Type { get; set; }
    }
}
