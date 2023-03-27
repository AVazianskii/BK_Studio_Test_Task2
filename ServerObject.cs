using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace _2
{
    public class ServerObject
    {
        private byte[] bytesRead;
        private List<IPEndPoint> clients;
        private IPEndPoint ListenIpEndPoint;
        private IPAddress SenderAdress;



        public void Run()
        {
            try
            {
                Console.WriteLine("Сервер запущен...");

                while (true)
                {
                    ReceiveMessage(clients, out bytesRead, out SenderAdress);
                    BroadcastMessage(bytesRead, clients, SenderAdress);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n" + ex.InnerException);
            }
        }



        public ServerObject()
        {
            ListenIpEndPoint = new IPEndPoint(IPAddress.Any, 14000); 
            clients = new List<IPEndPoint>();
        }



        private void BroadcastMessage(byte[] Message, List<IPEndPoint> clients, IPAddress address)
        {
            using (Socket tcpSender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    // Отправляем принятое сообщение от одного клиента всем остальным клиентам, подключенным к данному серверу чата
                    foreach (var client in clients)
                    {
                        if (client.Address.ToString() != address.ToString())
                        {
                            if (tcpSender.Connected == false)
                            {
                                tcpSender.Connect(client.Address, 14001);
                            }
                            // Если сообщение не является сервисным, то транслируем его
                            if (Encoding.UTF8.GetString(Message) != " ")
                            {
                                tcpSender.Send(Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\n" + ex.InnerException);
                }
            }
        }
        private void ReceiveMessage(List<IPEndPoint> clients, out byte[] finalArray, out IPAddress senderAdress)
        {
            using (Socket tcpListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                tcpListener.Bind(ListenIpEndPoint);
                tcpListener.Listen(10);
                using (var tcpClient = tcpListener.Accept())
                {
                    // массив для принятых байтов
                    bytesRead = new byte[256];

                    // получаем адрес пользователя для дальнейшей работы 
                    EndPoint remoteIP = tcpClient.RemoteEndPoint;
                    // получаем сообщение
                    int numberOfBytes = tcpClient.ReceiveFrom(bytesRead, ref remoteIP);

                    // Отсекаем нулевые байты, которые не несут информации
                    List<byte> finalList = new List<byte>();
                    foreach (byte element in bytesRead)
                    {
                        if (element != 0x00)
                        {
                            finalList.Add(element);
                        }
                    }
                    finalArray = finalList.ToArray();

                    IPEndPoint remoteFullIP = remoteIP as IPEndPoint;
                    senderAdress = remoteFullIP.Address;

                    // добавляем пользоателя в коллекцию активных пользователей, если он подключился впервые
                    CheckForNewClient(clients, remoteFullIP);
                    // Выводим принятое сообщение в консоль для контроля программы
                    Console.WriteLine(Encoding.UTF8.GetString(bytesRead));  
                }
            }
        }
        private  void CheckForNewClient(List<IPEndPoint> clients, IPEndPoint newClient)
        {
            bool flag = true;
            foreach (var client in clients)
            {
                if (client.Address.ToString() == newClient.Address.ToString())
                {
                    flag = false;
                }
            }
            if (flag)
            {
                clients.Add(newClient);
            }
        }

    }
}
