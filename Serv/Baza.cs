using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serv
{
    internal class Baza
    {
        MySqlConnection connection;
        public Baza(string ip, string db_user, string database, string db_password)
        {
            string db = "server=" + ip + ";user=" + db_user + ";database=" + database + ";password=" + db_password + ";";
            connection = new MySqlConnection(db);  //Connection (Подключение) 
            connection.Open();
        }
        public int request_auth(string login, string password)
        {
            int h = 0;
            string request = "SELECT `id`, `login`, `status` FROM `users` WHERE `login` = '" + login + "' and `pass` = '" + password + "'";
            MySqlCommand command = new MySqlCommand(request, connection);   // Send request (Отправляем запрос)
            MySqlDataReader reader = command.ExecuteReader();               // Read request answer (Читаем ответ)

            while (reader.Read())
            {
                if (reader[1].ToString() == login)
                {
                    h = 1;
                }
                if (reader[2].ToString() == "1" && reader[1].ToString() == login)
                {
                    h = 2;
                }
            }
            reader.Close();

            if (h == 1)
            {
                request = "UPDATE `users` SET `status` = '1' WHERE `login` = '" + login + "'";
                command = new MySqlCommand(request, connection);
                command.ExecuteNonQuery();
            }

            return h;
        }
        public bool request_registr(string login, string password)
        {
            bool f = false;
            string request = "SELECT `login` FROM `users` WHERE `login` = '" + login + "'";
            MySqlCommand command = new MySqlCommand(request, connection);
            MySqlDataReader reader = command.ExecuteReader();               // Send request (Отправляем запрос)

            while (reader.Read())
            {
                if (reader[0].ToString() == login)
                    f = true;
                else f = false;
            }
            reader.Close();

            if (f)
                return false;
            request = "INSERT INTO `users` (`login`, `pass`, `status`) VALUES ('" + login + "', '" + password + "', '1')";
            command = new MySqlCommand(request, connection);
            command.ExecuteNonQuery();                                     // Send request (Отправляем запрос)
        }

        public bool request_logout(string login)
        {
            try
            {
                string request = "UPDATE `users` SET `status` = '0' WHERE `login` = '" + login + "'";
                MySqlCommand command = new MySqlCommand(request, connection);
                command.ExecuteNonQuery();                                      // Отправляем запрос
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex);
            }
            return true;
        }

        public List<(int, int, string)> request_UsersList()
        {
            List<(int, int, string)> users = new List<(int, int, string)>();
            string request = "SELECT * FROM `users`";
            MySqlCommand command = new MySqlCommand(request, connection);
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add((Convert.ToInt32(reader[0]), Convert.ToInt32(reader[3]), reader[1].ToString()));
            }
            reader.Close();
            return users;
        }

        public List<(string, string, int)> request_MessageList(string user1, string user2)
        {
            List<(string, string,int)> messages = new List<(string, string, int)>();
            string request;
            if (user2 == "All")
            {
                request = "SELECT `sender`, `conversation`, `id` FROM `messages` WHERE `reader` = 'All'";
                MySqlCommand command = new MySqlCommand(request, connection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add((Convert.ToString(reader[0]), Convert.ToString(reader[1]), Convert.ToInt32(reader[2])));
                }
                reader.Close();
            }
            else
            {
                request = "SELECT `sender`, `conversation`, `id` FROM `messages` WHERE `sender` = '" + user1 + "' AND `reader` = '" + user2 + "' or `sender` = '" + user2 + "' AND `reader` = '" + user1 + "'";
                MySqlCommand command = new MySqlCommand(request, connection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add((Convert.ToString(reader[0]), Convert.ToString(reader[1]), Convert.ToInt32(reader[2])));
                }
                reader.Close();
            }

            return messages;
        }

        public void request_MessageAdd(string sender, string recipient, string message)
        {
            string request = "INSERT INTO `messages` (`sender`, `reader`, `conversation`) VALUES ('" + sender + "', '" + recipient + "', '" + message + "')";
            MySqlCommand command = new MySqlCommand(request, connection);
            command.ExecuteNonQuery();
        }

        public void request_MessageDelete(string id, string user)
        {
            string request = "DELETE FROM `messages` WHERE `id` = '" + id + "' AND `sender` = '" + user + "'";
            MySqlCommand command = new MySqlCommand(request, connection);
            command.ExecuteNonQuery();
        }

        public void request_UserDelete(string user)
        {
            Console.WriteLine("User deleted: " + user);
            string request = "DELETE FROM `users` WHERE `login` = '" + user + "'";
            MySqlCommand command = new MySqlCommand(request, connection);
            command.ExecuteNonQuery();
        }

        public void close()
        {
            connection.Close();
        }
    }
}
