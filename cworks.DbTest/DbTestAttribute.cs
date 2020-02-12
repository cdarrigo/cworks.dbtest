using System;
using Xunit;
using Xunit.Sdk;

namespace cworks.DbTest
{
    [XunitTestCaseDiscoverer("Xunit.Sdk.FactDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DbTestAttribute : FactAttribute
    {
        
    }
}