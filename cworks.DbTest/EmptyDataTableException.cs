using System;
using System.Data;
using Xunit.Sdk;

namespace cworks.DbTest
{
    public class EmptyDataTableException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="T:Blackbaud.TMS.DataManagement.Service.UnitTests.DbTest.EmptyDataTableException" /> class.
        /// </summary>
        public EmptyDataTableException(DataTable dataTable)
            : base("Assert.Empty() Failure")
        {
            this.DataTable = dataTable;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        /// <summary>The DataTable that failed the test.</summary>
        public DataTable DataTable { get; }

        /// <inheritdoc />
        public override string Message => base.Message + Environment.NewLine + "DataTable: ";
    }
}