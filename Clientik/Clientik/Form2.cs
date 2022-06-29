using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Clientik
{
    public partial class Form2 : Form
    {
        private string login;
        private string password;
        Socket socket;
        private string ip;
        private int port;
        List<(string, string, string)> listMes = new List<(string, string, string)>();
        public void preload(Socket socket, string login, string password, string ip, int port)
        {
            this.socket = socket;
            this.login = login;
            this.password = password;
            this.ip = ip;
            this.port = port;
        }
        public Form2()
        {
            InitializeComponent();
        }
        private void send_mes(string message, Socket socket)
        {
            //  Sending 
            byte[] data = Encoding.Unicode.GetBytes(message);
            socket.Send(data);
        }
        private string[] ans_mes(Socket socket)
        {
            byte[] buffer = new byte[1024]; //Buffer for received data ( Буфер для получаемых данных)
            var size = 0;                   // Number of bytes received (Количество полученных байтов)
            StringBuilder answer = new StringBuilder();
            do
            {
                size = socket.Receive(buffer, buffer.Length, 0); //**
                answer.Append(Encoding.Unicode.GetString(buffer, 0, size));  
            }
            while (socket.Available > 0);
            return answer.ToString().Split('*');
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            this.Show();
            listView1.Items[0].Focused = true;   // set default chat ((All))
            listView1.Items[0].Selected = true;
            listView1.Items[0].EnsureVisible();
            toolStripMenuItem1.Text = login;
            UpdateData();
            timer1.Enabled = true;
        }
        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            send_mes("B", socket);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void SendButton_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text, recipient = label1.Text;
            if (label1.Text == "Многопользовательский чат")
                recipient = "All";
            send_mes("G*" + login + "*" + recipient + "*" + message, socket);
            textBox1.Text = "";
        }
        private void ListView_Click(object sender, EventArgs e)
        {
            UpdateData();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateData();
        }
        public void UpdateData()
        {
            // List of users
            string current = "Многопользовательский чат";
            bool f = false;
            for (int i = 0; i < listView1.Items.Count; i++)
                if (listView1.Items[i].Selected)
                    current = listView1.Items[i].Text;
            int usersnumber; // number of users
            listView1.Items.Clear();
            listView1.Items.Add("Многопользовательский чат");
            send_mes("F*" + login, socket);
            usersnumber = Convert.ToInt32(ans_mes(socket)[0]);
            for (int i = 1; i < usersnumber + 1; i++)
            {
                int pict = 0;
                send_mes(" ", socket);
                string[] ans = ans_mes(socket);
                if (ans[1] != login)
                {
                    if (ans[0] == "1")
                        pict = 1;
                    listView1.Items.Add(ans[1]).ImageIndex = pict;
                }
            }
            for (int i = 0; i < listView1.Items.Count; i++)
                if (listView1.Items[i].Text == current)
                {
                    listView1.Items[i].Selected = true;
                    f = true;
                }
            if (!f)
            {
                listView1.Items[0].Selected = true;
            }
            //                                   //
            //  Messages
            string currentmes = "0";
            bool s = false;
            if (listBox1.SelectedItems.Count > 0)
            {
                currentmes = listBox1.SelectedItem.ToString();
                s = true;
            }
            listBox1.Items.Clear();
            listMes.Clear();
            for (int i = 0; i < listView1.Items.Count; i++)
            {   
                if (listView1.Items[i].Selected) //  Show selected chat
                {
                    label1.Text = listView1.Items[i].Text;
                    if (listView1.Items[i].Text == "Многопользовательский чат")
                        send_mes("E*" + login + "*All", socket);
                    else
                        send_mes("E*" + login + "*" + listView1.Items[i].Text, socket);
                    string[] ans = ans_mes(socket);
                    int count = Convert.ToInt32(ans[0]);
                    for (int j = 0; j < count; j++)
                    {
                        send_mes(" ", socket);
                        ans = ans_mes(socket);
                        listMes.Add((ans[2], ans[0], ans[1]));
                        listBox1.Items.Add(ans[0] + ": " + ans[1]);
                    }
                    break;
                }
            }
            if (s)
            {
                for (int i = 0; i < listBox1.Items.Count; i++)
                {
                    if (listBox1.Items[i].ToString() == currentmes)
                    {
                        listBox1.SelectedIndex = i;
                    }
                }
            }
            //
        }
        private void UserDelToolStripMenuItem_Click(object sender, EventArgs e) //  user delete
        {
            send_mes("A*" + login, socket);
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            send_mes("H*" + Convert.ToInt32(listMes[listBox1.SelectedIndex].Item1) + "*" + login, socket);
        }

        private void menuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
