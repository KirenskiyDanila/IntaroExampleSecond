using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;
using System.Configuration;

namespace IntaroExampleSecond
{
    class DatabaseConnection
    {
        private static string password = ConfigurationManager.AppSettings.Get("DBpassword");


        public static bool AddSubscription(string ChatId, string name, string owner) // добавление подписки в БД
        {

            if (Check(ChatId,name,owner))
            {
                 return false;

            }
            string InsertRequest = @"INSERT INTO Subscribers(ChatId, name, owner)
                                VALUES(" + ChatId + ", '" + name + "', '" + owner + "');";

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;User Id=postgres; " +
             "Password=" + password + ";Database=IntaroExample;");

            conn.Open();

            NpgsqlCommand InsertCommand = new NpgsqlCommand(InsertRequest, conn);

            NpgsqlDataReader dr = InsertCommand.ExecuteReader();

            dr.Close();

            return true;
        }

        public static bool Check(string ChatId, string name, string owner) // проверка на наличие подписки в БД
        {
            string Request = "SELECT chatId FROM Subscribers WHERE chatId = " + ChatId + " AND owner='" + owner + "' AND name='" + name + "'";
            
            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;User Id=postgres; " +
             "Password=" + password + ";Database=IntaroExample;");

            conn.Open();

            NpgsqlCommand Command = new NpgsqlCommand(Request, conn);

            NpgsqlDataReader dr = Command.ExecuteReader();

            if (dr.HasRows) return true;

            dr.Close();

            return false;
        }

        public static bool DeleteSubscription(string ChatId, string name, string owner) // удаление подписки из БД
        {

            if (!Check(ChatId,name,owner)) return false;

            string DeleteRequest = @"DELETE FROM Subscribers WHERE chatId = " + ChatId + " AND owner = '" + owner + "' AND name = '" + name + "'";

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;User Id=postgres; " +
             "Password=" + password + ";Database=IntaroExample;");

            conn.Open();


            NpgsqlCommand DeleteCommand = new NpgsqlCommand(DeleteRequest, conn);

            NpgsqlDataReader dr = DeleteCommand.ExecuteReader();

            dr.Close();

            return true;
        }



        public static List<string> ListOfSubscriptions(string ChatId) // возвращает список подписок пользователя
        {
            List<string> list = new List<string>();

            string SelectRequest = "SELECT name, owner FROM Subscribers WHERE chatId = " + ChatId;

            NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;User Id=postgres; " +
             "Password=" + password + ";Database=IntaroExample;");

            conn.Open();

            NpgsqlCommand SelectCommand = new NpgsqlCommand(SelectRequest, conn);

            NpgsqlDataReader dr = SelectCommand.ExecuteReader();

            while (dr.Read())
            {
                string URL = "https://github.com/" + dr[1] + "/" + dr[0];
                list.Add(URL);
            }    

            return list;
        }

    }
}
