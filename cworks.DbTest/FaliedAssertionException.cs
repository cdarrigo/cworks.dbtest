using Xunit.Sdk;

namespace cworks.DbTest
{
    public class FaliedAssertionException : XunitException
    {

        private readonly string trimmedStackTrace ;
        // ReSharper disable once ConvertToAutoProperty
        public override string StackTrace => trimmedStackTrace;

        public FaliedAssertionException( XunitException inner):base(inner.Message)
        {

            // for now - restore the stack trace
            trimmedStackTrace = inner.StackTrace;
            // trim off the non-test related frames from the stack trace
            // we really only want to show the top frame (at position 1) 
            //trimmedStackTrace =new StackTrace(new StackTrace(inner,true).GetFrame(1)).ToString();
        }
    }
}