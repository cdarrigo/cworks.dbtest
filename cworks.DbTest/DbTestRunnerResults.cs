using System.Collections.Generic;
using System.Linq;

namespace cworks.DbTest
{
    public class DbTestRunnerResults
    {
        public DbInitializationResult InitializationResult { get; set; }
        public List<DbTestRunnerResult> TestResults { get; set; } = new List<DbTestRunnerResult>();
        public bool TestDisabled { get; set; } 
        public bool IsSuccessful => TestDisabled || (InitializationResult?.IsSuccessful ?? false) && (TestResults?.All(i => i.IsSuccessful) ?? true);
    }
}