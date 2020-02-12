using System;

namespace cworks.DbTest
{
    public class DbTestSetupException:Exception
    {
        public DbTestSetupException(string message):base(message)
        {
            
        }

        public DbTestSetupException(string message, Exception inner):base(message,inner)
        {
            
        }
    }
}