using System;
using System.Data;
using Xunit.Sdk;

namespace cworks.DbTest
{
    public class NotEmptyDataTableException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="T:Blackbaud.TMS.DataManagement.Service.UnitTests.DbTest.NotEmptyDataTableException" /> class.
        /// </summary>
        public NotEmptyDataTableException(DataTable dataTable)
            : base("Assert.NotEmpty() Failure")
        {
            this.DataTable = dataTable;
        }

        /// <summary>The DataTable that failed the test.</summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DataTable DataTable { get; }

        /// <inheritdoc />
        public override string Message => base.Message + Environment.NewLine + "DataTable: ";
    }
}