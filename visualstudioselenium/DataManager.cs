using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;

namespace suseso
{
    public class DataManager
    {
        private string connectionString;

        /// <summary>
        /// constructor for DataManager Class
        /// </summary>
        /// <param name="sConnectionString">Conection strign from app.config</param>
        public DataManager(string sConnectionString)
        {
            this.connectionString = sConnectionString;
        }

        /// <summary>
        /// generic function for execute SQLite command with SQLite BDDriver.
        /// non return data
        /// </summary>
        /// <param name="query">query for execution insert or update</param>
        public string setData(string query)
        {
            DataTable dt = new DataTable();
            Microsoft.Data.Sqlite.SqliteConnection connection;
            Microsoft.Data.Sqlite.SqliteCommand command;
            connection = new Microsoft.Data.Sqlite.SqliteConnection(this.connectionString);

            try
            {
                connection.Open();
                command = new Microsoft.Data.Sqlite.SqliteCommand(query, connection);
                dt.Load(command.ExecuteReader());
                connection.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                return "error";
            }
        }

        /// <summary>
        /// generic function for execute SQL command with SQL Server BDDriver.
        /// </summary>
        /// <param name="query">query for execution insert or update</param>
        public string setDataSQL(string query)
        {
            DataTable dt = new DataTable();
            SqlConnection connection;
            SqlCommand command;

            connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                dt.Load(command.ExecuteReader());
                connection.Close();
                return "ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
                return "error";
            }
        }


        /// <summary>
        /// generic function for execute SQL command with SQLite BDDriver.
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="connectionString"></param>
        /// <returns>return DataTable with result</returns>
        public DataTable getData(string query)
        {
            DataTable dt = new DataTable();
            Microsoft.Data.Sqlite.SqliteConnection connection;
            Microsoft.Data.Sqlite.SqliteCommand command;
            connection = new Microsoft.Data.Sqlite.SqliteConnection(connectionString);

            try
            {
                connection.Open();
                command = new Microsoft.Data.Sqlite.SqliteCommand(query, connection);
                dt.Load(command.ExecuteReader());
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
            }

            return dt;
        }

        /// <summary>
        /// generic function for execute SQL command with SQL SERVER BDDriver.
        /// </summary>
        /// <param name="query">query to execute</param>
        /// <param name="connectionString"></param>
        /// <returns>return DataTable with result</returns>
        public DataTable getDataSQL(string query)
        {
            DataTable dt = new DataTable();
            SqlConnection connection =  new SqlConnection(connectionString);
            SqlCommand command;

            try
            {
                connection.Open();
                command = new SqlCommand(query, connection);
                dt.Load(command.ExecuteReader());
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Fatal Error]\r\n" + ex.Message + "\r\n" + ex.StackTrace + "\r\n" + ex.InnerException + "\r\n" + ex.Source);
                Console.WriteLine("........Fail");
            }
            return dt;
        }
    }
}
