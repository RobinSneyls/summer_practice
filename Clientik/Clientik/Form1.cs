using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientik
{
    public partial class Form1 : Form
    {
        const string ip = "127.0.0.1";
        const int port = 7070;
        IPEndPoint tcpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);    // Port
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);    // Создание сокета
        private string login = "", password;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                if (socket.Connected)
                    send_mes("B*" + login);
            }
            catch (Exception ex) { }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try     // Trying connection
            {
                socket.Connect(tcpEndPoint); // Connect socket 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
                label1.Text = "Соединение с сервером не установлено!";
                button1.Enabled = false;
                button2.Enabled = false;
                return;
            }
        }
        private void send_mes(string message)
        {
            //  Sending
            byte[] data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            login = textBox1.Text;
            password = textBox2.Text;
            if (login != "" && password != "")  //  Check for empty(Проверка на пустоту)
            {
                if (login != "All")
                {
                    string message = "C*" + login + "*" + password;
                    send_mes(message);
                    string[] answer = ans_mes();
                    if (answer[0] == "1")
                    {
                        // Join user
                        label1.Text = "";
                        Form2 newMes = new Form2();
                        newMes.preload(socket, login, password, ip, port);
                        newMes.Show();
                        this.Hide();
                    }
                    else if (answer[0] == "2")
                    {
                        label1.Text = "Пользователь уже авторизован!";
                    }
                    else if (answer[0] == "0")
                    {
                        label1.Text = "Неверное имя пользователя или пароль!";
                    }
                }
                else
                {
                    label1.Text = "Имя пользователя занято!";
                }
            }
            else
            {
                label1.Text = "Заполните поля!";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            login = textBox1.Text;
            password = textBox2.Text;
            if (login != "" && password != "")  //  Check field for empty (проверка полей на пустоту)
            {
                if (login != "All")
                {
                    string message = "D*" + login + "*" + password;
                    send_mes(message);
                    if (ans_mes()[0] == "1")
                    {
                        // Joined user
                        label1.Text = "";
                        Form2 newMes = new Form2();
                        newMes.preload(socket, login, password, ip, port);
                        newMes.Show();
                        this.Hide();
                    }
                    else
                    {
                        label1.Text = "Пользователь уже существует!";
                    }
                }
                else
                {
                    label1.Text = "Имя пользователя занято!";
                }
            }
            else
            {
                label1.Text = "Заполните поля!";
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private string[] ans_mes()
        {
            byte[] buffer = new byte[1024]; // Buffer for received data ( Буфер для получаемых данных)
            var size = 0;                   // Number of bytes received (Количество полученных байтов)
            StringBuilder answer = new StringBuilder();
            do
            {
                size = socket.Receive(buffer, buffer.Length, 0);
                answer.Append(Encoding.Unicode.GetString(buffer, 0, size));  // Из сообщения берем непустые байты
            }
            while (socket.Available > 0);
            return answer.ToString().Split('*');
        }

    }
}
