using System;
using System.Data;
using System.Data.SqlClient;

namespace cworks.DbTest
{
    public interface IDbScaffolder
    {
        DbInitializationResult InitializeDb(IDbTestRunnerConfiguration config, IDbTestRunnerContext context);
        void DropDatabase(IDbTestRunnerContext context, IDbTestRunnerConfiguration config);
        
    }

    public class SqlServerDbScaffolder : IDbScaffolder
    {

        /// <summary>
        /// Drops the database specified in the context's DBName property
        /// </summary>
        public void DropDatabase(IDbTestRunnerContext context, IDbTestRunnerConfiguration config)
        {
            if (context == null)
            {
                return;
            }

            try
            {
                var connString = ProduceConnectionString(config, "master");
                using (var conn = new SqlConnection(connString))
                {

                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;

                        cmd.CommandText = $"Drop Database {context.DbName}";
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        /// <summary>
        /// Determines if the specified database exists in the configured server
        /// </summary>
        protected bool DoesDatabaseExist(IDbTestRunnerConfiguration config, string dbName)
        {
            try
            {
                using (var conn = new SqlConnection(ProduceConnectionString(config, dbName, false, 3)))
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }

            }
            catch (SqlException)
            {
                return false;
            }

        }


        /// <summary>
        /// Provisions the database specified in Context.DbName
        /// Creates the DB if it does not exist and runs all
        /// data context migrations against the database.
        /// </summary>
        public DbInitializationResult InitializeDb(IDbTestRunnerConfiguration config, IDbTestRunnerContext context) 
        {
            if (context == null)
            {

                throw new ArgumentNullException(nameof(context));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var result = new DbInitializationResult();
            try
            {
                
                if (string.IsNullOrEmpty(context.DbName) || !DoesDatabaseExist(config, context.DbName))
                {
                    result.Logs.Add($"Creating new Test database {context.DbName}.");
                    CreateDatabase(config,context);
                    result.WasDatabaseCreated = true;
                }
                else
                {
                    result.Logs.Add($"Using existing database {context.DbName}.");
                }



                try
                {
                    context.ConnectionString = ProduceConnectionString(config, context.DbName);
                    InitializeSchemaAndData(config, context, result);
                    if (!result.IsSuccessful)
                    {
                        result.Logs.Add("Failed to initialize db schema and data.");
                        result.IsSuccessful = false;
                    }
                }
                catch (Exception e)
                {

                    result.Exception = e;
                    result.Logs.Add("Failed to initialize db schema and data.");
                    result.IsSuccessful = false;
                }

                if (result.IsSuccessful)
                {
                    result.Logs.Add($"Database {context.DbName} has been prepared for tests.");
                    result.IsSuccessful = true;
                }
            }
            catch (Exception e)
            {
                result.Exception = e;
                result.Logs.Add($"Error attempting to Initialize DB. {e}");
                result.IsSuccessful = false;
            }

            return result;
        }

        protected virtual void InitializeSchemaAndData(IDbTestRunnerConfiguration config, IDbTestRunnerContext context, DbInitializationResult result)
        {
            result.IsSuccessful = true;
        }

        /// <summary>
        /// Creates the database specified in Context.DbName
        /// </summary>
        protected void CreateDatabase(IDbTestRunnerConfiguration config, IDbTestRunnerContext context)
        {

            
            var connStr = this.ProduceConnectionString(config, "master");
            string sql = $"CREATE DATABASE {context.DbName}";

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }



        
        /// <summary>
        /// Produces the ado.net connection string using the server and credentials
        /// specified in the configuration
        /// </summary>
        /// <returns></returns>
        protected string ProduceConnectionString(IDbTestRunnerConfiguration config, string dbName = null, bool useConnectionPooling = true, int? connectionTimeoutSeconds = null)
        {
            return $"Data Source={config.ConnectionInfo.ServerName};" +
                   $"Initial Catalog={dbName ?? config.DbName};" +
                   (config.ConnectionInfo.UseIntegratedSecurity ? "Integrated Security=SSPI;" : $"User Id={config.ConnectionInfo.UserName};Password={config.ConnectionInfo.Password};") +
                   (!useConnectionPooling ? " Pooling=false;" : "") +
                   (connectionTimeoutSeconds.HasValue ? $" Connection Timeout={connectionTimeoutSeconds};" : "") +
                   "MultipleActiveResultSets=True";
        }

        
    }

}