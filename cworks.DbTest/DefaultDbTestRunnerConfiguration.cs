namespace cworks.DbTest
{
    public class DefaultDbTestRunnerConfiguration : DbTestRunnerConfigurationBase
    {
        public DefaultDbTestRunnerConfiguration(string server) : base(server)
        {
        }

        public DefaultDbTestRunnerConfiguration(string server, string username, string password) : base(server, username, password)
        {
        }
    }
}