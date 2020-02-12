namespace cworks.DbTest
{
    public enum DataContextCommitPoint
    {
        /// <summary>
        /// Don't commit data context
        /// </summary>
        None,
        /// <summary>
        /// Commit before running any t-sql commands
        /// </summary>
        BeforeRunningSql,
        
        /// <summary>
        /// Commit after running t-sql commands
        /// </summary>
        AfterRunningSql
    }
}