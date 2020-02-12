using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace cworks.DbTest
{
    // ReSharper disable UnusedMemberInSuper.Global
    public interface IDbTestRunnerContext
    {
        
        string ConnectionString { get; set; }
        string DbName { get; set; }
        // ReSharper disable once UnusedMember.Global
        IConfiguration Configuration { get; set; }
        IDbScaffolder DbScaffolder { get; set; }
        IServiceProvider ServiceProvider { get; set; }
        // ReSharper disable once UnusedMember.Global
        Dictionary<object, object> State { get; set; }
     
        bool CommitTransactionScope { get; set; }

         Func<string, string, SqlParameter> ProduceSqlParametersForCustomTypes { get; set; }

    }
}