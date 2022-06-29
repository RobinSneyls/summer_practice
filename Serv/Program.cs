using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Serv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string ip = "127.0.0.1";
            const int port = 7070;
            ThreadPool.SetMinThreads(2, 2);
            ThreadPool.SetMaxThreads(10, 10);   //Кол-во потоков
            Server server = new Server(ip, port);
            server.Start();
            string cons;
            while (true)
            {
                cons = Console.ReadLine();
                if (cons == "stop")
                {
                    server.Stop();
                }
            }
        }
    }
}
