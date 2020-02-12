using System;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace cworks.DbTest
{

    // ReSharper disable UnusedMember.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    public interface IDbTestRunnerConfiguration
    {
        /// <summary>
        /// FOR FUTURE
        /// Specifies the degrees of parallelism when running dbTests
        /// Default is 1
        /// </summary>
        int NumberOfParallelTests { get; set; }

//        /// <summary>
//        /// Should the test runner stop running tests when the first test fails?
//        /// Default is true.
//        /// </summary>
//        bool StopTestExecutionOnFailure { get; set; }

        /// <summary>
        /// Should the test runner drop the test database if any tests fail?
        /// Default is true
        /// </summary>
        bool DropDatabaseOnFailure { get; set; }

        /// <summary>
        /// Should the test runner drop the test database if all the tests succeed?
        /// Default is true
        /// </summary>
        bool DropDatabaseOnSuccess { get; set; }

        /// <summary>
        /// DB Connection info.
        /// This can be set manually, but probably easiest to use
        /// one of the constructor overloads for this class to set
        /// the connection info automatically.
        /// </summary>
        DbTestRunnerConnectionInfo ConnectionInfo { get; set; }

        /// <summary>
        /// If the test DB already exists, you can specify its name here.
        /// </summary>
        string DbName { get; set; }

        /// <summary>
        /// instance of the XUnit ITestOutputHelper
        /// used for relaying test run output to the XUnit Test framework
        /// </summary>
        ITestOutputHelper TestOutputHelper { get; set; }

        Func<bool> Enabled { get; set; }

        //IDbTestRunnerConfiguration ProduceDefaultConfiguration(IConfiguration systemConfig);
        void Validate();

    }

    /// <summary>
    /// Configuration for the DBTest runs.
    /// </summary>
    public abstract class DbTestRunnerConfigurationBase : IDbTestRunnerConfiguration
    {

        /// <summary>
        /// Configures the DB Tests to use the specified
        /// DB server, and to use integrated security
        /// </summary>
        protected DbTestRunnerConfigurationBase(string server)
        {
            this.ConnectionInfo.ServerName = server;
            this.ConnectionInfo.UseIntegratedSecurity = true;            
        }

        /// <summary>
        /// Configures the DB Tests to use the specified
        /// DB Server, and to use username and password credentials 
        /// </summary>
        protected DbTestRunnerConfigurationBase(string server, string username, string password)
        {
            this.ConnectionInfo.ServerName = server;
            this.ConnectionInfo.UseIntegratedSecurity = false;
            this.ConnectionInfo.UserName = username;
            this.ConnectionInfo.Password = password;
            
        }

        /// <summary>
        /// default constructor
        /// </summary>
        protected DbTestRunnerConfigurationBase()
        {
            this.ConnectionInfo = new DbTestRunnerConnectionInfo();
        }
        /// <summary>
        /// FOR FUTURE
        /// Specifies the degrees of parallelism when running dbTests
        /// Default is 1
        /// </summary>
        public int NumberOfParallelTests { get; set; } = 1;
//        
//        /// <summary>
//        /// Should the test runner stop running tests when the first test fails?
//        /// Default is true.
//        /// </summary>
//        public bool StopTestExecutionOnFailure { get; set; } = true;
        
        /// <summary>
        /// Should the test runner drop the test database if any tests fail?
        /// Default is true
        /// </summary>
        public bool DropDatabaseOnFailure { get; set; } = true;
        
        /// <summary>
        /// Should the test runner drop the test database if all the tests succeed?
        /// Default is true
        /// </summary>
        public bool DropDatabaseOnSuccess { get; set; } = true;
        
        /// <summary>
        /// DB Connection info.
        /// This can be set manually, but probably easiest to use
        /// one of the constructor overloads for this class to set
        /// the connection info automatically.
        /// </summary>
        public DbTestRunnerConnectionInfo ConnectionInfo { get; set; } = new DbTestRunnerConnectionInfo();
        
        /// <summary>
        /// If the test DB already exists, you can specify its name here.
        /// </summary>
        public string DbName { get; set; }
        
        /// <summary>
        /// instance of the XUnit ITestOutputHelper
        /// used for relaying test run output to the XUnit Test framework
        /// </summary>
        public ITestOutputHelper TestOutputHelper { get; set; }

        public Func<bool> Enabled { get; set; } = () => true;
        //public abstract IDbTestRunnerConfiguration ProduceDefaultConfiguration(IConfiguration systemConfig);

        public virtual void Validate()
        {
            if (this.ConnectionInfo == null)
                throw new DbTestSetupException("ConnectionInfo cannot be null.");
            if (string.IsNullOrEmpty(this.ConnectionInfo.ServerName))
                throw new DbTestSetupException("ConnectionInfo.ServerName must be defined.");
            if (!this.ConnectionInfo.UseIntegratedSecurity && string.IsNullOrEmpty(this.ConnectionInfo.UserName))
                throw new DbTestSetupException("ConnectionInfo.Username must be specified when ConnectionInfo.UseIntegratedSecurity is true.");
            if (!this.ConnectionInfo.UseIntegratedSecurity && string.IsNullOrEmpty(this.ConnectionInfo.Password))
                throw new DbTestSetupException("ConnectionInfo.Password must be specified when ConnectionInfo.UseIntegratedSecurity is true.");
        }


        protected static T ProduceTestConfiguration<T>(IConfiguration systemConfig) where T : IDbTestRunnerConfiguration, new()
        {
            var config = new T
            {
                ConnectionInfo = new DbTestRunnerConnectionInfo
                {
                    ServerName = systemConfig["DbTest:Connect:ServerName"],
                    UseIntegratedSecurity = systemConfig.ReadBool("DbTest:Connect:UseIntegratedSecurity")
                }
            };

            if (!config.ConnectionInfo.UseIntegratedSecurity)
            {
                config.ConnectionInfo.UserName =systemConfig["DbTest:Connect:UserName"];
                config.ConnectionInfo.Password =   systemConfig["DbTest:Connect:Password"];
            }

            config.DbName = systemConfig["DbTest:DbName"]??config.DbName;
            config.DropDatabaseOnFailure= systemConfig.ReadBool("DbTest:DropDatabaseOnFailure", config.DropDatabaseOnFailure);
            config.DropDatabaseOnSuccess= systemConfig.ReadBool("DbTest:DropDatabaseOnSuccess", config.DropDatabaseOnFailure);
            var enabled = systemConfig.ReadBoolNullable("DbTest:Enabled");
            if (enabled.HasValue)
                config.Enabled = () => enabled.Value;

            return config;
        }
    }
}