using NLog;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbIndexingUtil
{
    /// <summary>
    /// Объект для работы с БД.
    /// </summary>
    public class DbManufacturer
    {
        #region Поля

        /// <summary>
        /// Логгер.
        /// </summary>
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Команда для извлечения.
        /// </summary>
        private const string extractionCommand = "SELECT * FROM BOOKS";

        /// <summary>
        /// Имя провайдера подключения к Oracle БД.
        /// </summary>
        private const string orclConnectionName = "Oracle.ManagedDataAccess.Client.OracleClientFactory";

        /// <summary>
        /// Имя провайдера подключения к MSSQL БД.
        /// </summary>
        private const string mssqlConnectionName = "System.Data.SqlClient";

        /// <summary>
        /// Имя провайдера подключения к PostgreSQL БД.
        /// </summary>
        private const string npgsqlConnectionName = "Npgsql";

        /// <summary>
        /// Объект подключения к БД.
        /// </summary>
        private readonly DbConnection connection;

        /// <summary>
        /// Объект команды доступ к БД.
        /// </summary>
        private readonly DbCommand command;

        #endregion

        #region Методы

        /// <summary>
        /// Извлечь данные их БД.
        /// </summary>
        public void ExtractFromDb()
        {
            try
            {
                connection.Open();
                command.CommandText = extractionCommand;

                var reader = command.ExecuteReader();

                var books = new List<Book>();
                
                while (reader.Read())                
                    Console.WriteLine($"{reader.GetInt32(0)}\t{reader.GetString(1)}\t{reader.GetString(2)}");
                
                reader.Dispose();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Создаётся новый экземпляр подключения к БД.
        /// <para>Исключения: <see cref="UnknownProviderNameException"/>.</para>
        /// </summary>
        public DbManufacturer()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ConnectionString;

            switch (ConfigurationManager.ConnectionStrings["CONNECTION_STRING"].ProviderName)
            {
                case orclConnectionName:
                    connection = new OracleConnection(connectionString);
                    break;
                case mssqlConnectionName:
                    connection = new SqlConnection(connectionString);
                    break;
                case npgsqlConnectionName:
                    connection = new NpgsqlConnection(connectionString);
                    break;
                default:
                    throw new UnknownProviderNameException();
            }

            command = connection.CreateCommand();
        }

        #endregion


    }
}
