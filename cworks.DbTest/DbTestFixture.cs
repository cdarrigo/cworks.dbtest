using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ClassNeverInstantiated.Global

namespace cworks.DbTest
{

    public interface IStronglyTypedDbTestSetup
    {
        SqlParameter ProduceSqlParametersForCustomTypes(string name, string type);
        IServiceProvider ProduceServiceProvider();
        void TearDownDatabase(IDbTestRunnerContext context, IDbTestRunnerConfiguration config, bool allTestsWereSuccessful);
    }

    

    public class DbTestFixture : IDisposable
        {
            // ReSharper disable once StaticMemberInGenericType
            private IServiceProvider ServiceProvider { get;  }
            private readonly IDbTestRunnerContext context;
            private readonly IDbTestRunnerConfiguration config;
            private readonly IDbScaffolder scaffolder;

            public bool AllTestsSuccessful { get; set; } = true;

            public IDbTestRunnerContext Context => context;
            public IDbTestRunnerConfiguration Configuration => config;
            private readonly IStronglyTypedDbTestSetup dbTestSetup;

            // ReSharper disable once UnusedMember.Global
            public DbTestFixture()
            {
                
                this.dbTestSetup= DbTestSetupFinder.DbTestSetup;
                if (dbTestSetup == null)
                {
                    throw new Exception("Failed to create test fixture. Unable to determine DbTest setup class.");
                }
                this.ServiceProvider = dbTestSetup.ProduceServiceProvider();

                this.config = ServiceProvider.GetService<IDbTestRunnerConfiguration>();
                this.scaffolder = ServiceProvider.GetService<IDbScaffolder>();
                this.context = ServiceProvider.GetService<IDbTestRunnerContext>();
                this.context.ProduceSqlParametersForCustomTypes =dbTestSetup.ProduceSqlParametersForCustomTypes;

                // if we're not supposed to run DbTests, then don't bother setting up the runner. 
                if (!this.config.Enabled()) return;


                // ensure the db exists, has latest migrations, etc. 
                var initializationResult = InitializeDatabase();
                if (!initializationResult.IsSuccessful)
                {
                    throw new DbTestException("Failed to intialize database for tests.");
                }
            }



            /// <summary>
            /// Creates the test database if doesn't already exist
            /// Runs migrations to bring the database up to latest schema
            /// Runs any database initialization code for the tests to execute 
            /// </summary>
            private DbInitializationResult InitializeDatabase()
            {
                // Create the database and run the migrations
                return this.scaffolder.InitializeDb(config, context);
            }

            
            

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    this.dbTestSetup?.TearDownDatabase(context,Configuration,this.AllTestsSuccessful);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

        }

    public static class DbTestSetupFinder
    {
        public static IStronglyTypedDbTestSetup DbTestSetup
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(i => !i.FullName.StartsWithAnyCi("System", "Microsoft", "XUnit", "mscorlib", "Newtonsoft", "netstandard", "testhost", typeof(DbTestSetupFinder).Assembly.FullName,
                        "Anonymously Hosted", "SOS.", "Remotion", "WindowsBase"))
                    .ToArray();
                foreach (var assembly in assemblies)
                {
                    var type = assembly.GetTypes().FirstOrDefault(t => !t.IsAbstract && typeof(IStronglyTypedDbTestSetup).IsAssignableFrom(t));
                    if (type != null)
                    {
                        var instance = Activator.CreateInstance(type) as IStronglyTypedDbTestSetup;
                        return instance;
                    }
                }

                return null;
            }
        }
    }
}