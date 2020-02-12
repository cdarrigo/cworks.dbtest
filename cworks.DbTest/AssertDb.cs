using System;
using System.Data;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Global

namespace cworks.DbTest
{
     public static class AssertDb
    {
       
        /// <summary>Verifies that a data table is empty.</summary>
        /// <param name="dataTable">The data table to be inspected</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when the data table is null</exception>
        /// <exception cref="T:Blackbaud.TMS.DataManagement.Service.UnitTests.DbTest.EmptyDataTableException">Thrown when the data table is not empty</exception>
        public static void Empty(DataTable dataTable)
        {
            GuardArgumentNotNull(nameof (dataTable), dataTable);
            if (dataTable.Rows.Count != 0)
                throw new EmptyDataTableException(dataTable);
            
        }
        
        /// <summary>Verifies that a data table is not empty.</summary>
        /// <param name="dataTable">The data table to be inspected</param>
        /// <exception cref="T:System.ArgumentNullException">Thrown when a null data table is passed</exception>
        /// <exception cref="T:Blackbaud.TMS.DataManagement.Service.UnitTests.DbTest.NotEmptyDataTableException">Thrown when the data table is empty</exception>
        public static void NotEmpty(DataTable dataTable)
        {
            GuardArgumentNotNull(nameof (dataTable), dataTable);
            if (dataTable.Rows.Count == 0) 
                throw new NotEmptyDataTableException(dataTable);
        }
        
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private static void GuardArgumentNotNull(string argName, object argValue)
        {
            if (argValue == null)
                throw new ArgumentNullException(argName);
        }

        
        /// <summary>
        /// Asserts that the data columns returned in the data table
        /// match by name to the properties of the the model type specified
        /// </summary>
        public static void AssertHasModelProperties<TModel>(this DataTable dt, ITestOutputHelper testOutputHelper=null)
        {
            var missingProperties = dt.GetMissingModelProperties<TModel>();
            foreach (var pi in missingProperties)
            {
                var msg = $"Data shape '{typeof(TModel).Name}' is missing a column to map to property '{pi.Name}' ({pi.PropertyType.Name})";
                if (testOutputHelper == null)
                    Console.WriteLine(msg);
                else
                {
                    testOutputHelper.WriteLine(msg);
                }
            }
            Assert.Empty(missingProperties);
                
        }

    }
}