using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace cworks.DbTest
{

    public abstract class DbTestSetup :   ICollectionFixture<DbTestFixture>,
        IStronglyTypedDbTestSetup 
    {
//        protected IDbTestRunnerConfiguration ProduceConfiguration()
//        {
//            // spin up a default config
//            // pass the config to ConfigureTests and get back the configuration
//            // validate the configuration
//            // if its not valid, throw a fatal exception
//            // return the configuration 
//
//            var defaultConfiguration = ProduceDefaultConfiguration();
//
//            var config = this.ConfigureTests(defaultConfiguration);
//            return config;
//            //var config = ProduceDbTestConfiguration();
//            //return config;
//        }

        

        /// <summary>
        /// Here is where you configure your DB Test environment.
        /// Specify the server name, login credentials, optional dbName and test run behaviors.
        /// </summary>
        protected abstract IDbTestRunnerConfiguration ConfigureDbTests(IDbTestRunnerConfiguration config);
        
        /// <summary>
        /// You can generate sql parameters for Custom Sql types by overriding this method
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="type">Custom Sql Type</param>
        /// <returns></returns>
        public virtual  SqlParameter ProduceSqlParametersForCustomTypes(string name, string type)
        {
            return null;
        }

        protected virtual void RegisterServices(IServiceCollection services, IDbTestRunnerContext context, IDbTestRunnerConfiguration config, IDbScaffolder scaffolder)
        {
            
        }

        //public IDbTestRunnerConfiguration DbTestConfiguration => ProduceConfiguration();
        protected virtual IDbTestRunnerContext ProduceContext()
        {
            return new DbTestRunnerContext();
        }

        protected virtual IDbScaffolder ProduceScaffolder()
        {
            return new SqlServerDbScaffolder();
        }
        public IServiceProvider ProduceServiceProvider()
        {

            var services = new ServiceCollection();
            services.AddLogging();

            var configuration = ProduceValidConfiguration();
            services.AddSingleton(configuration);

            var context = ProduceContext();
            services.AddSingleton(context);

            var scaffolder = ProduceScaffolder();

            services.AddSingleton(scaffolder);

            this.RegisterServices(services,  context, configuration, scaffolder);

            var serviceProvider = services.BuildServiceProvider();

            context.ServiceProvider = serviceProvider;
            context.DbScaffolder = scaffolder;
            context.DbName = SetDbName(configuration);
            return serviceProvider;
        }

        protected abstract IDbTestRunnerConfiguration ProduceDefaultConfiguration(IConfiguration systemConfig);

        private IDbTestRunnerConfiguration ProduceValidConfiguration()
        {
            var systemConfig = ProduceSystemConfiguration();

            // call into the abstract method to produce a default configuration
            var config = this.ProduceDefaultConfiguration(systemConfig);

            // let the user override/tweak the configuration
            config = this.ConfigureDbTests(config);


            AfterDbTestsConfigured(config);
            // validate the configuration 
            config.Validate();

            return config;
        }

        protected abstract void AfterDbTestsConfigured(IDbTestRunnerConfiguration config);

        /// <summary>
        /// Produces an Microsoft.Configuration instance
        /// from an existing AppSettings.json file in the current folder
        /// and environment vars.
        /// </summary>
        private IConfiguration ProduceSystemConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfiguration config = builder.Build();
            return config;
        }

        private string SetDbName(IDbTestRunnerConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.DbName))
                return configuration.DbName;
            return ProduceUniqueDatabaseName();
        }


        /// <summary>
        /// Produces a unique database name
        /// comprised on the current date time
        /// </summary>
        protected virtual string ProduceUniqueDatabaseName()
        {
            var now = DateTime.Now;
            //var guidSuffix = Guid.NewGuid().ToString().Substring(24, 9);
            var ticksSuffix = DateTime.Now.Ticks;
            var timestamp = $"{now.Year}{now.Month:00}{now.Year}_{now.Hour:00}{now.Minute:00}{now.Second:00}";
            return $"DbTest_{timestamp}_{ticksSuffix}";
        }

        public abstract void TearDownDatabase(IDbTestRunnerContext context, IDbTestRunnerConfiguration config, bool allTestsWereSuccessful);
        

       
    }
    
}