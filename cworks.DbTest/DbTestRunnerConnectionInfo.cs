namespace cworks.DbTest
{
    /// <summary>
    /// Test Database connection info
    /// </summary>
    public class DbTestRunnerConnectionInfo
    {
        public string ServerName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseIntegratedSecurity { get; set; } = true;
    }
}