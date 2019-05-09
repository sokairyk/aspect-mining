using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectMining.Core.Database
{
    public class SQLCompactEdition35
    {
        public static string ConnectionString
        {
            get
            {
                String cnn = ConfigurationManager.AppSettings["SQLCE3.5"];

                if (cnn == null)
                    throw new ArgumentNullException("App" + ".config file does not contain the appropriate entry 'SQLCE3.5' at appSettings section.");

                return String.Format("Data Source={0}; File Mode=Shared Read; Max Buffer Size=262140; Persist Security Info=False", cnn);
            }
        }

        public static DataTable ExecuteDataTable(String query)
        {
            DataTable data = null;

            using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(query, ConnectionString))
            {
                // The adapter will open and close the connection
                data = new DataTable();
                adapter.Fill(data);
            }

            return data;
        }

        public static void ExecuteNonQuery(String query)
        {
            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCeCommand command = new SqlCeCommand())
                {
                    command.Connection = connection;

                    command.CommandText = query;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }
    }
}
