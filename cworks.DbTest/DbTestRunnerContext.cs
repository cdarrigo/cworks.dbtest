using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace cworks.DbTest
{
    // ReSharper disable UnusedMember.Global
    /// <summary>
    /// Context object holding the data
    /// about the configured test run
    /// </summary>
    public class DbTestRunnerContext:IDbTestRunnerContext 
    {
        /// <summary>
        /// ADO.NET DB Connection string
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// DB name
        /// </summary>
        public string DbName { get; set; }
        
        /// <summary>
        /// .NET Runtime configuration
        /// (from Appsettings.json, etc)
        /// </summary>
        public IConfiguration Configuration { get; set; }
        
        /// <summary>
        /// Instance of the configured service provider
        /// </summary>
        public IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Instance of scaffold provider
        /// </summary>
        public IDbScaffolder DbScaffolder { get; set; }
        /// <summary>
        /// Stateful dictionary to hold any state between DbTests
        /// within the DbTestRunner run.
        /// </summary>
        public Dictionary<object,object> State  {get;set;} = new Dictionary<object, object>();
        
//        /// <summary>
//        /// Instance of the produced data context
//        /// </summary>
//        public TDataContext DataContext { get; set; }
        
        /// <summary>
        /// Ioc Service scope used to produce the data context
        /// </summary>
        public IServiceScope ServiceScope { get; set; }
        
        /// <summary>
        /// instance of the XUnit ITestOutputHelper
        /// used for relaying test run output to the XUnit Test framework
        /// </summary>
        public ITestOutputHelper TestOutputHelper { get; set; }
        
        /// <summary>
        /// List of DbTests that will be executed as part of this test run
        /// </summary>
        public List<IDbTest> TestsToExecute { get; set; } = new List<IDbTest>();

        /// <summary>
        /// For diagnostic purposes only
        /// </summary>
        /// <returns></returns>
        public bool CommitTransactionScope { get; set; } = false;


        public Func<string, string, SqlParameter> ProduceSqlParametersForCustomTypes { get; set; }
    }
}