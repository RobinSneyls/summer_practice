using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serv
{
    internal class Server
    {
        public EndPoint Ip;     // IP
        int Port;             // Port
        Socket Listener;        // Socket (Сокет)
        public bool Active;     // Server status (Состояние сервера)
        public Server(string ip, int port)
        {
            this.Port = port;
            this.Ip = new IPEndPoint(IPAddress.Parse(ip), Port);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Start()     // Start (Старт сервера)
        {
            try
            {
                if (!Active)
                {
                    Listener.Bind(Ip);      // связываем сокет с локальной точкой, по которой будем принимать данные
                    Listener.Listen(16);    // Start listen( начинаем прослушивание)
                    Active = true;
                    Console.WriteLine("Сервер запущен. Ожидание подключений...");
                    while (Active)
                    {
                        ThreadPool.QueueUserWorkItem(
                            new WaitCallback(ClientThread),
                            Listener.Accept());
                    }
                }
                else
                {
                    Console.WriteLine("Server was started");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Stop()      // Stop server
        {
            if (Active)
            {
                Listener.Close();
                Active = false;
            }
            else
                Console.WriteLine("Server was stopped");
        }

        public void ClientThread(object client)
        {
            new Client((Socket)client);
        }


    }

    internal class Client
    {
        Socket client;

        public string[] ans_mes()
        {
            byte[] buffer = new byte[1024]; //  Buffer for received data ( Буфер для получаемых данных)
            int size = 0;                   // Number of bytes received (Количество полученных байтов)
            StringBuilder answer = new StringBuilder();
            do
            {
                size = client.Receive(buffer);
                answer.Append(Encoding.Unicode.GetString(buffer, 0, size)); // берем непустые байты
            }
            while (client.Available > 0);

            return answer.ToString().Split('*');
        }

        public void send_mes(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.Send(data);
        }

        public Client(Socket client)
        {
            this.client = client;
            string login = "";
            bool exit = false;
            Baza connection;
            try
            {
                while (!exit)
                {
                    string[] answer = ans_mes();

                    bool f = false;
                    string flag = "0";

                    switch (answer[0])
                    {
                        case "A":     // delete
                            connection = new Baza("localhost", "root", "information", "root");
                            connection.request_UserDelete(answer[1]);
                            connection.close();
                            exit = true;
                            break;

                        case "B":     // Exit
                            connection = new Baza("localhost", "root", "information", "root");
                            connection.request_logout(login);
                            connection.close();

                            exit = true;
                            break;

                        case "C":     // Sing in
                            connection = new Baza("localhost", "root", "information", "root");
                            int fl = connection.request_auth(answer[1], answer[2]);
                            connection.close();
                            if (fl == 1)
                            {
                                flag = "1";
                                login = answer[1];
                            }
                            else if (fl == 2)
                            {
                                flag = "2";
                            }
                            send_mes(flag);
                            break;

                        case "D":     // registration
                            connection = new Baza("localhost", "root", "information", "root");
                            f = connection.request_registr(answer[1], answer[2]);
                            connection.close();
                            if (f)
                            {
                                flag = "1";
                                login = answer[1];
                            }
                            send_mes(flag);
                            break;

                        case "E": // Messages
                            connection = new Baza("localhost", "root", "information", "root");
                            List<(string, string, int)> messages = new List<(string, string, int)>();
                            messages = connection.request_MessageList(answer[1], answer[2]);
                            connection.close();
                            send_mes(messages.Count.ToString());
                            for (int i = 0; i < messages.Count; i++)
                            {
                                ans_mes();
                                string m = messages[i].Item1 +"*"+ messages[i].Item2 + "*" + messages[i].Item3;
                                send_mes(m);
                            }
                            break;

                        case "F":     // List of users
                            connection = new Baza("localhost", "root", "information", "root");
                            List<(int, int, string)> users = new List<(int, int, string)>();
                            users = connection.request_UsersList();
                            connection.close();
                            send_mes(users.Count.ToString());
                            for (int i = 0; i < users.Count; i++)
                            {
                                ans_mes();
                                string m = users[i].Item2 + "*" + users[i].Item3;
                                send_mes(m);
                            }
                            break;

                        case "G": // Send messages
                            connection = new Baza("localhost", "root", "information", "root");
                            connection.request_MessageAdd(answer[1], answer[2], answer[3]);
                            connection.close();
                            break;

                        case "H": // Delete messages
                            connection = new Baza("localhost", "root", "information", "root");
                            connection.request_MessageDelete(answer[1], answer[2]);
                            connection.close();
                            break;
                    }
                }
            }
            catch (Exception ex) { }
            connection = new Baza("localhost", "root", "information", "root");
            connection.request_logout(login);
            connection.close();
            Console.WriteLine("Клиент " + login + " разорвал соединение");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            return;
        }
    }
}
