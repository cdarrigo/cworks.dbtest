using System;
using System.Collections.Generic;

namespace cworks.DbTest
{
    // ReSharper disable  UnusedAutoPropertyAccessor.Global
    // ReSharper disable  CollectionNeverQueried.Global
    public class DbInitializationResult
    {
        /// <summary>
        /// logging messages returned from the Initialization task
        /// </summary>
        public List<string> Logs { get; } = new List<string>();
        
        /// <summary>
        /// Exception message thrown by the initialization task
        /// </summary>
        public Exception Exception { get; set; }
        
        /// <summary>
        /// Success indicator. 
        /// </summary>
        public bool IsSuccessful { get; set; }

        public bool WasDatabaseCreated { get; set; }
        
    }
}