using System.Net;
using System.Net.Sockets;

namespace _2
{
    public class Program
    {
        static void Main(string [] args)
        {
            ServerObject server = new ServerObject();
            server.Run();
        }
    }
}
