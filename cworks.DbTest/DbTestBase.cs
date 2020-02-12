using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace cworks.DbTest
{

    /// <summary>
    /// Use this one
    /// </summary>
    [Collection(DbTestConstants.CollectionName)]
    public abstract class DbTestBase : IDbTest
    {
        protected readonly ITestOutputHelper TestOutputHelper;
        protected readonly DbTestFixture TestFixture;

        protected virtual bool Enabled => true;

        protected DbTestBase(ITestOutputHelper outputHelper, DbTestFixture testFixture)
        {
            this.TestOutputHelper = outputHelper;
            this.TestFixture = testFixture;
        }

        /// <summary>
        /// By default, the test name is the class name
        /// </summary>
        public virtual string Name => this.GetType().Name;






        protected abstract IDisposable ProduceDbHandle();

        // ReSharper disable once NotAccessedField.Local
        private IDisposable handleInstance;

        /// <summary>
        /// public method for running this test
        /// </summary>
        protected DbTestRunnerResult ExecuteArrangeActAssert()
        {
            var context = this.TestFixture.Context;
            var result = new DbTestRunnerResult {TestName = this.Name};
            TransactionScope transactionScope = null;

            try
            {
                using (transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {

                    try
                    {
                        using (var handle = ProduceDbHandle())
                        {
                            this.handleInstance = handle;
                            var arrangeResult = ExecuteArrange(context, result, handle);
                            if (arrangeResult.IsSuccessful)
                            {
                                var actResult = ExecuteAct(context, result, handle);

                                if (actResult.IsSuccessful)
                                {
                                    ExecuteAssert(context, result, actResult.Data, handle);
                                }
                            }

                            this.handleInstance = null;
                        }
                    }
                    finally
                    {
                        this.handleInstance = null;
                        if (context.CommitTransactionScope)
                        {
                            transactionScope.Complete();
                        }
                    }

                }
            }
            // catch and re-throw assertions
            catch (XunitException)
            {
                throw;
            }
            catch (Exception e)
            {
                result = DbTestRunnerResult.Failure("Error running DbTest", e);
            }
            finally
            {
                transactionScope?.Dispose();

            }

            DumpRunLog(result);

            return result;

        }

        /// <summary>
        /// Arrange the state of the data.
        /// Set up the database with the seeded data you'll need to exercise the db object under test.
        /// Data set up here is not accessible to other tests.
        /// </summary>
        protected abstract SqlRequest OnArrange(IDbTestRunnerContext context, IDisposable dbHandle, ITestOutputHelper testOutputHelper);

        private DbTestSqlResult ExecuteArrange(IDbTestRunnerContext context, DbTestRunnerResult result, IDisposable dbHandle)
        {

            var request = OnArrange(context, dbHandle, this.TestOutputHelper);
            result.TestPhase = "ARRANGE";
            return ExecuteTestPhase(result, "ARRANGE", request);
        }


        /// <summary>
        /// ACT on the state of the data.
        /// This is where you should exercise the db object under test.
        /// The data returned from this code will be presented to the Assert method.
        /// </summary>
        protected abstract SqlRequest OnAct(IDbTestRunnerContext context, IDisposable dbHandle, ITestOutputHelper testOutputHelper);

        private DbTestSqlResult ExecuteAct(IDbTestRunnerContext context, DbTestRunnerResult result, IDisposable dbHandle)
        {

            var request = OnAct(context, dbHandle, this.TestOutputHelper);
            result.TestPhase = "ACT";
            return ExecuteTestPhase(result, "ACT", request);
        }



        protected abstract void OnAssert(DataTable data, IDisposable dbHandle, DataTable[] allData, ITestOutputHelper testOutputHelper);

        /// <summary>
        /// Invokes the ASSERT hook method
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        private void ExecuteAssert(IDbTestRunnerContext context, DbTestRunnerResult result, List<DataTable> data, IDisposable dbHandle)
        {
            try
            {

                result.TestPhase = "ASSERT";
                result.IsSuccessful = true;
                result.Logs.Add("ASSERT Phase: Running test Assertions.");
                this.OnAssert(data == null || !data.Any() ? null : data[0], dbHandle, data?.ToArray(), this.TestOutputHelper);
                result.Logs.Add("ASSERT Phase: All assertions passed.");
            }
            catch (XunitException)
            {
                // we actually just want to re-throw the current exception to maintain the stack trace 
                throw;
            }
            catch (Exception e)
            {
                // non-assertion exception occurred. 
                result.Logs.Add($"Error asserting state of data. {e}");
                result.Exception = e;
                result.IsSuccessful = false;
            }

        }

        protected void DumpRunLog(DbTestRunnerResult result)
        {

            if (result == null) return;

            this.TestOutputHelper?.WriteLine($"[{DateTime.Now:s}]{result.TestName}");
            foreach (var msg in result.Logs)
            {
                TestOutputHelper?.WriteLine(msg);
            }

            if (result.Exception != null)
            {
                TestOutputHelper?.WriteLine(result.Exception.ToString());
            }
        }


        protected virtual int CommitChanges()
        {
            return 0;
        }


        protected virtual SqlConnection GetDbConnection()
        {
            return null;
        }

        /// <summary>
        /// Executes the sql requests 
        /// </summary>
        protected DbTestSqlResult ExecuteTestPhase(DbTestRunnerResult result, string phaseName, SqlRequest request)
        {
            var phaseResult = new DbTestSqlResult();
            try
            {

                if (request == null)
                {
                    return DbTestSqlResult.Success;
                }

                // if we're supposed to commit the data context changes before running the sql, do so now.
                if (request.DataContextCommitPoint == DataContextCommitPoint.BeforeRunningSql)
                {
                    var contextRowsAffected = this.CommitChanges();
                    result.Logs.Add($"{phaseName} Phase: Commited Data Context prior to executing sql.  Rows Affected: {contextRowsAffected}");
                }

                if (request.IsEmpty)
                {
                    return DbTestSqlResult.Success;
                }


                // Run the sql, we have different execution strategies based on the command types
                switch (request.CommandType)
                {
                    case CommandType.Text:
                        phaseResult = this.ExecuteSqlTextRequest(request, result, phaseName);
                        break;
                    case CommandType.StoredProcedure:
                        phaseResult = this.ExecuteSqlStoredProcedureRequest(request, result, phaseName);
                        break;
                    case CommandType.TableDirect:
                        phaseResult = this.ExecuteSqlTableDirectRequest(request, result, phaseName);
                        break;
                    default:
                        throw new Exception("Unsupported Command Type");
                }

                // if there was a problem running the sql command, stop executing now
                if (!phaseResult.IsSuccessful)
                {
                    return phaseResult;
                }


                // if we're supposed to commit the data context changes after running the sql, do so now.
                if (request.DataContextCommitPoint == DataContextCommitPoint.AfterRunningSql)
                {
                    var contextRowsAffected = this.CommitChanges();
                    result.Logs.Add($"{phaseName} Phase: Commited Data Context prior to executing sql.  Rows Affected: {contextRowsAffected}");
                }

                // if we got this far, the test executed successfully, set the return value
                phaseResult.IsSuccessful = true;
            }
            catch (Exception e)
            {
                // runtime exception executing the test. 
                result.Exception = e;
                result.Logs.Add($"Error attempting to execute {phaseName} sql. {e.GetType().Name} - {e.Message}");
                phaseResult.IsSuccessful = false;
                throw;
            }

            // set the overall result success status based on how this command was executed
            result.IsSuccessful = phaseResult.IsSuccessful;
            result.TestPhase = phaseName;
            return phaseResult;
        }


        /// <summary>
        /// When the command Type is SqlText.
        /// Run any inline sql. 
        /// </summary>
        private DbTestSqlResult ExecuteSqlTextRequest(SqlRequest request, DbTestRunnerResult result, string phaseName)
        {
            var sqlResult = new DbTestSqlResult();
            try
            {
                // Create a new command using the same connection as the data context
                using (var cmd = GetDbConnection().CreateCommand())
                {

                    // build up the sql command object
                    // Set the command parameters
                    foreach (var p in request.Parameters.Select(i => i.Value))
                    {
                        result.Logs.Add($"{phaseName} Sql Parameter:{p.ParameterName} = {p.Value}");
                        cmd.Parameters.Add(p);
                    }

                    cmd.CommandText = request.SqlToExecute;
                    result.Logs.Add($"{phaseName} Sql: {cmd.CommandText}");
                    cmd.CommandType = CommandType.Text;
                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    // Execute the command via a data reader and use it to load data tables; one for each result set returned from the sql. 
                    using (var rdr = cmd.ExecuteReader())
                    {
                        do
                        {
                            var dt = new DataTable();
                            dt.Load(rdr);
                            sqlResult.Data.Add(dt);
                            result.Logs.Add($"{phaseName} Phase: DataTable[{sqlResult.Data.Count - 1}] read {dt.Rows.Count} rows(s).");
                        } while (!rdr.IsClosed && rdr.NextResult());
                    }

                    result.Logs.Add($"{phaseName} Phase: Read {sqlResult.Data.Count} result set(s).");
                }

                sqlResult.IsSuccessful = true;
            }
            catch (Exception e)
            {
                sqlResult.Exception = e;
                sqlResult.IsSuccessful = false;
                throw;
            }

            return sqlResult;
        }

        /// <summary>
        /// used when the CommandType is TableDirect.
        /// </summary>
        private DbTestSqlResult ExecuteSqlTableDirectRequest(SqlRequest request, DbTestRunnerResult result, string phaseName)
        {


            var sqlResult = new DbTestSqlResult();
            try
            {
                // Create a new command using the same connection as the data context
                using (var cmd = this.GetDbConnection().CreateCommand())
                {

                    // Set the command parameters
                    foreach (var p in request.Parameters.Select(i => i.Value))
                    {
                        result.Logs.Add($"{phaseName} Sql Parameter:{p.ParameterName} = {p.Value}");
                        cmd.Parameters.Add(p);
                    }

                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    // build up the sql command object
                    cmd.CommandText = $"Select * From {request.SqlToExecute}";
                    result.Logs.Add($"{phaseName} Table Direct Sql: {cmd.CommandText}");
                    cmd.CommandType = CommandType.Text;



                    // Execute the command via a data reader and use it to load data tables; one for each result set returned from the sql.
                    using (var rdr = cmd.ExecuteReader())
                    {
                        do
                        {
                            var dt = new DataTable();
                            dt.Load(rdr);
                            sqlResult.Data.Add(dt);
                            result.Logs.Add($"{phaseName} Phase: DataTable[{sqlResult.Data.Count - 1}] read {dt.Rows.Count} rows(s).");
                        } while (!rdr.IsClosed && rdr.NextResult());
                    }

                    result.Logs.Add($"{phaseName} Phase: Read {sqlResult.Data.Count} result set(s).");
                }

                sqlResult.IsSuccessful = true;
            }
            catch (Exception e)
            {
                sqlResult.Exception = e;
                sqlResult.IsSuccessful = false;
            }

            return sqlResult;
        }

        /// <summary>
        /// Used when CommandType is StoredProcedure
        /// </summary>
        private DbTestSqlResult ExecuteSqlStoredProcedureRequest(SqlRequest request, DbTestRunnerResult result, string phaseName)
        {
            var sqlResult = new DbTestSqlResult();
            try
            {
                //   var dataTable = new DataTable();
                var dataset = new DataSet();
                // Create a new command using the same connection as the data context
                using (var cmd = GetDbConnection().CreateCommand())
                {
                    if (cmd.Connection.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }

                    // build up the sql command object
                    cmd.CommandText = request.SqlToExecute;
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Set the command parameters
                    foreach (var p in request.Parameters.Select(i => i.Value))
                        cmd.Parameters.Add(p);
                    result.Logs.Add($"{phaseName} Stored Procedure: {cmd.CommandText}");
                    // Execute the command via a data adapter and use it to load a data set
                    using (var dataAdapter = new SqlDataAdapter(cmd))
                    {
                        //dataAdapter.Fill(dataTable);
                        dataAdapter.Fill(dataset);

                    }
                }

                // populate the sqlResult data tables with the tables filled by the adapter
                sqlResult.Data.AddRange(dataset.Tables.Cast<DataTable>());
                //sqlResult.Data.Add(dataTable);
                sqlResult.IsSuccessful = true;
            }
            catch (Exception e)
            {
                sqlResult.Exception = e;
                sqlResult.IsSuccessful = false;
            }

            return sqlResult;
        }



        protected SqlParameter[] GetParametersForDbObject(string objectName, IDbTestRunnerContext runnerContext)
        {
            var parameters = new List<SqlParameter>();
            var sql = @" Select p.name, t.name 
                               From sys.parameters p
                               inner join sys.types t on p.system_type_id = t.system_type_id 
                               where Object_Id = OBJECT_ID(@objname)
                               order by p.parameter_id";
            var pObjName = new SqlParameter("@objname", objectName);
            using (var cmd = GetDbConnection().CreateCommand())
            {
                if (cmd.Connection.State != ConnectionState.Open)
                {
                    cmd.Connection.Open();
                }

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.Parameters.Add(pObjName);
                using (var rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        var parameterName = rdr[0].ToString();
                        var parameterType = rdr[1].ToString();
                        parameters.Add(this.GetSqlParameterWithDefaultValue(parameterName, parameterType, runnerContext));
                    }
                }
            }

            return parameters.ToArray();
        }

        private SqlParameter GetSqlParameterWithDefaultValue(string name, string type, IDbTestRunnerContext runnerContext)
        {
            switch (type)
            {
                case "bit":
                    return new SqlParameter(name, false);
                case "int":
                case "byte":
                case "tinyint":
                case "smallint":
                case "bigint":
                case "numeric":
                    return new SqlParameter(name, -1);
                case "decimal":
                case "float":
                case "smallmoney":
                    return new SqlParameter(name, -1m);
                case "date":
                case "datetime":
                case "datetime2":
                case "smalldatetime":
                case "timestamp":

                    return new SqlParameter(name, System.Data.SqlTypes.SqlDateTime.MinValue.Value.AddSeconds(1));
                case "text":
                case "ntext":
                case "varchar":
                case "nvarchar":
                case "char":
                case "nchar":
                case "xml":
                    return new SqlParameter(name, "");
                default:
                {
                    var customTypeParameter = ProduceSqlParametersForCustomTypes(name, type, runnerContext);
                    return customTypeParameter ?? new SqlParameter(name, null);
                }
            }
        }

        protected virtual SqlParameter ProduceSqlParametersForCustomTypes(string name, string type, IDbTestRunnerContext runnerContext)
        {
            return runnerContext.ProduceSqlParametersForCustomTypes?.Invoke(name, type);
        }


        /// <summary>
        /// Holds the result of executing data command
        /// </summary>
        protected class DbTestSqlResult
        {
            public static DbTestSqlResult Success => new DbTestSqlResult {IsSuccessful = true};


            public Exception Exception { get; set; }
            public List<DataTable> Data { get; } = new List<DataTable>();
            public bool IsSuccessful { get; set; }
        }


        /// <summary>
        /// This is the actual fact test that the dbTests will be invoking when
        /// the xUnit test runner runs. 
        /// </summary>
        [Fact]
        //    [DebuggerStepThrough]
        public virtual void ExecuteTest()
        {
            if (!this.Enabled)
            {
                this.TestOutputHelper.WriteLine("Test was not executed because it was configured not to run. Test has been disabled. Test.Enabled returned false.");
                return;
            }

            Assert.True(this.TestFixture != null, "TestFixture is null. Be sure the DbTest class is using a Fixture Collection attribute ([Collection(\"DbTests\")]");

            if (this.TestFixture.Configuration == null)
            {
                this.TestOutputHelper.WriteLine("Test was not executed because it was configured not to run. Test fixture is missing configuration. Tests will not run without a configuration for the fixture.");
                return;
            }

            if (!this.TestFixture.Configuration.Enabled())
            {
                this.TestOutputHelper.WriteLine("Test was not executed because it was configured not to run. Test has been disabled. Test fixture has been disabled.");
                return;
            }

            try
            {
                var result = ExecuteArrangeActAssert();
                if (!result.IsSuccessful)
                    this.TestFixture.AllTestsSuccessful = false;

                Assert.True(result.IsSuccessful);

            }
            catch (Exception)
            {
                this.TestFixture.AllTestsSuccessful = false;
                throw;
            }
        }
    }

}