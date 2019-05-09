using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace AspectMining.Core.Database
{
    /// <summary>
    ///   Delegates handling of SQLDataReader object to another function
    /// </summary>
    /// <remarks>
    ///  Used by <see cref="Sql.WhileReader(System.Data.SqlClient.SqlConnection,string,System.Collections.Generic.List{System.Data.SqlClient.SqlParameter},Lighthouse.Web.DataReaderConsumer)"/> function
    /// </remarks>
    /// <param name="reader">Current row reader</param>
    public delegate void DataReaderConsumer(SqlDataReader reader);

    /// <summary>
    ///    Functions for accessing SQL Database
    /// </summary>
    /// <remarks>
    ///  Use these functions in 2 ways. (Appropriate overloaded methods exist
    /// 
    /// <list type="table">
    /// <listheader>
    /// <term>"Mode"</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>One connection mode</term>
    /// <description>Use this mode in scenarios with extensivly lagre number of queries or nested executions (Data from one sql statement are used by another statement in a loop. The same connection is beiing used</description>
    /// </item>
    /// <item>
    /// <term>Connect and close mode</term>
    /// <description>Opens and closes SqlConnection within the function. Recomended for most ASP.NET usage scenarios</description>
    /// </item>
    /// </list>
    /// </remarks>
    public class SQLServer
    {
        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        /// <exception cref="ArgumentNullException">Raised if approprite record was not founf in the application part of web.config file</exception>
        public static string ConnectionString
        {
            get
            {
                ConnectionStringSettings cnn = ConfigurationManager.ConnectionStrings["SQLServer"];
                if (cnn == null)
                    throw new ArgumentNullException("Web" + ".config file does not contain the appropriate entry 'DBAccessConnection' at connections section.");

                return cnn.ConnectionString;
            }
        }

        /// <summary>
        /// Execute the Sql statement and read all records.
        /// </summary>
        /// <param name="statement">The Sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <param name="consume">Delegate function that handles current record.</param>
        /// <example><code>
        ///     DbSql.WhileReader(
        ///        "select * from users where name like @param", 
        ///        new List&lt;SqlParameter&gt;(new[] { new SqlParameter("@param","aUsersName"), }), 
        ///        (r) => { 
        ///          if (r["name"].ToString() == "test")
        ///            Debug.WriteLine("ok")
        ///          else
        ///             Debug.WriteLine("not ok")
        ///     });
        /// </code></example>
        public static void WhileReader(string statement, List<SqlParameter> parameters, DataReaderConsumer consume)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                                consume(reader);
                            reader.Close();
                        }
                    }
                    cmd.Parameters.Clear();
                }
            }
        }

        /// <summary>
        /// Execute the Sql Stored Procedure and read all records.
        /// </summary>
        /// <param name="statement">The Stored Procedure Name.</param>
        /// <param name="parameters">The Stored Procedure parameters.</param>
        /// <param name="consume">Delegate function that handles current record.</param>
        /// <example><code>
        ///     DbSql.WhileReader(
        ///        "GetUsers", 
        ///        new List&lt;SqlParameter&gt;(new[] { new SqlParameter("@UserName","aUsersName"), }), 
        ///        (r) => { 
        ///          if (r["name"].ToString() == "test")
        ///            Debug.WriteLine("ok")
        ///          else
        ///             Debug.WriteLine("not ok")
        ///     });
        /// </code></example>
        public static void WhileStoredProcReader(string statement, List<SqlParameter> parameters, DataReaderConsumer consume)
        {
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                                consume(reader);
                            reader.Close();
                        }
                    }
                    cmd.Parameters.Clear();
                }
            }
        }


        /// <summary>
        /// Execute the Sql statement and read all records.
        /// </summary>
        /// <param name="cnn">The connection object.</param>
        /// <param name="statement">The Sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <param name="consume">Delegate function that handles current record.</param>
        /// <example><code>
        ///     DbSql.WhileReader(
        ///        "select * from users where name like @param", 
        ///        new List&lt;SqlParameter&gt;(new[] { new SqlParameter("@param","aUsersName"), }), 
        ///        (r) => { 
        ///          if (r["name"].ToString() == "test")
        ///            Debug.WriteLine("ok")
        ///          else
        ///             Debug.WriteLine("not ok")
        ///     });
        /// </code></example>
        public static void WhileReader(SqlConnection cnn, string statement, List<SqlParameter> parameters, DataReaderConsumer consume)
        {            
            using (SqlCommand cmd = new SqlCommand(statement, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                            consume(reader);
                        reader.Close();
                    }
                }
                cmd.Parameters.Clear();
            }
        }

        /// <summary>
        /// Executes the sql statement and return a single value.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A signle value</returns>
        public static object ExecuteScalar(SqlConnection cnn, string statement, List<SqlParameter> parameters)
        {
            object res = null;
            using (SqlCommand cmd = new SqlCommand(statement, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);
                res = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            return res;
        }

        /// <summary>
        /// Executes the sql statement and return a single value.
        /// </summary>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A signle value</returns>
        public static object ExecuteScalar(string statement, List<SqlParameter> parameters)
        {
            object res = null;
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                    res = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the stored procedure and return a signle value
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>The value returned by the stored procedure</returns>
        public static object ExecuteScalarStoredProcedure(string storedProcedureName, List<SqlParameter> parameters)
        {
            object res = null;
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                    cmd.CommandType = CommandType.StoredProcedure;
                    res = cmd.ExecuteScalar();
                    cmd.Parameters.Clear();
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the stored procedure and return a signle value
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>The value returned by the stored procedure</returns>
        public static object ExecuteScalarStoredProcedure(SqlConnection cnn, string storedProcedureName, List<SqlParameter> parameters)
        {
            object res = null;
            using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);
                cmd.CommandType = CommandType.StoredProcedure;
                res = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
            }
            return res;
        }

        /// <summary>
        /// Executes the sql statement.
        /// </summary>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string statement, List<SqlParameter> parameters)
        {
            int res = -1;
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                    res = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the sql statement.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(SqlConnection cnn, string statement, List<SqlParameter> parameters)
        {
            int res = -1;
            using (SqlCommand cmd = new SqlCommand(statement, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);
                res = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            return res;
        }


        /// <summary>
        /// Executes the non query stored procedure.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns></returns>
        public static int ExecuteNonQueryStoredProc(string storedProcedureName, List<SqlParameter> parameters)
        {
            int res = -1;
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);
                    res = cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the non query stored procedure.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The SQL parameters.</param>
        /// <returns></returns>
        public static int ExecuteNonQueryStoredProc(SqlConnection cnn, string storedProcedureName, List<SqlParameter> parameters)
        {
            int res = -1;
            using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);
                res = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
            }
            return res;
        }



        /// <summary>
        /// Executes the Sql statement and return the results as DataSet.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection cnn, string statement, List<SqlParameter> parameters)
        {
            DataSet res = new DataSet();

            using (SqlCommand cmd = new SqlCommand(statement, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);

                using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                {
                    adp.Fill(res);
                    cmd.Parameters.Clear();
                }
            }

            return res;
        }

        /// <summary>
        /// Executes the Sql statement and return the results as DataSet.
        /// </summary>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A DataSet</returns>
        public static DataSet ExecuteDataSet(string statement, List<SqlParameter> parameters)
        {
            DataSet res = new DataSet();

            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(res);
                        cmd.Parameters.Clear();
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the Sql statement and return the results as DataTable.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection cnn, string statement, List<SqlParameter> parameters)
        {
            DataTable res = new DataTable();

            using (SqlCommand cmd = new SqlCommand(statement, cnn))
            {
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);

                using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                {
                    adp.Fill(res);
                    cmd.Parameters.Clear();
                }
            }

            return res;
        }


        /// <summary>
        /// Executes the Sql statement and return the results as DataTable.
        /// </summary>
        /// <param name="statement">The sql statement.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>A DataTable</returns>
        public static DataTable ExecuteDataTable(string statement, List<SqlParameter> parameters)
        {
            DataTable res = new DataTable();

            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                using (SqlCommand cmd = new SqlCommand(statement, cnn))
                {
                    if (parameters != null)
                        foreach (SqlParameter param in parameters)
                            cmd.Parameters.Add(param);

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        adp.Fill(res);
                        cmd.Parameters.Clear();
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns the results as Datatable.
        /// </summary>
        /// <param name="cnn">The connection string.</param>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>The results</returns>
        public static DataTable ExecuteDataTableStoredProcedure(SqlConnection cnn, string storedProcedureName, List<SqlParameter> parameters)
        {
            DataTable res = new DataTable();

            using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                if (parameters != null)
                    foreach (SqlParameter param in parameters)
                        cmd.Parameters.Add(param);

                SqlDataAdapter adp = new SqlDataAdapter();
                adp.SelectCommand = cmd;
                adp.Fill(res);
                cmd.Parameters.Clear();
            }
            return res;
        }

        /// <summary>
        /// Executes the specified stored procedure and returns the results as Datatable.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.</param>
        /// <param name="parameters">The sql parameters.</param>
        /// <returns>The results</returns>
        public static DataTable ExecuteDataTableStoredProcedure(string storedProcedureName, List<SqlParameter> parameters)
        {
            DataTable res = new DataTable();

            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Open();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(storedProcedureName, cnn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        if (parameters != null)
                            foreach (SqlParameter param in parameters)
                                cmd.Parameters.Add(param);

                        SqlDataAdapter adp = new SqlDataAdapter();
                        adp.SelectCommand = cmd;
                        adp.Fill(res);
                        cmd.Parameters.Clear();
                    }
                }
                finally
                {
                    cnn.Close();
                }
            }
            return res;
        }
    }

}


