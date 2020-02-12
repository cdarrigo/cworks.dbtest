using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace cworks.DbTest
{
    // ReSharper disable UnusedMember.Global
    public class DbTestRunnerResult
    {

        public static DbTestRunnerResult Success(string message = null)
        {
            var result = new DbTestRunnerResult
            {
                IsSuccessful = true,
            };
            if (!string.IsNullOrEmpty(message))
                result.Logs.Add(message);
            
            return result;
        }
        public static DbTestRunnerResult Failure(string message, Exception e = null)
        {
            var result = new DbTestRunnerResult
            {
                Exception = e,
                IsSuccessful = false
            };
            result.Logs.Add($"Error - {message}");
            return result;
        }
        
        public string TestName { get; set; }
        public List<string> Output { get; set; } = new List<string>();
        // ReSharper disable once CollectionNeverQueried.Global
        public List<string> Logs { get; set; } = new List<string>();
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string TestPhase { get; set; }
        public Exception Exception { get; set; }
        public XunitException FailedAssertionException { get; set; }
        public bool IsSuccessful { get; set; }
    }
}