using System;

namespace cworks.DbTest
{
    public class DbTestException : Exception
    {
        public DbTestException(string message):base(message)
        {
            
        }

        public DbTestException(string message, Exception inner): base(message,inner)
        {
            
        }
    }
}