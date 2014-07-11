using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace Shard.Tests
{
    public class TraceAttribute : BeforeAfterTestAttribute
    {
        private readonly Stopwatch _sw;

        public TraceAttribute()
        {
            _sw = new Stopwatch();
        }
        public override void Before(MethodInfo methodUnderTest)
        {
            _sw.Restart();
        }

        public override void After(MethodInfo methodUnderTest)
        {
            _sw.Stop();
            
            var className = methodUnderTest.DeclaringType.FullName;
            var methodName = methodUnderTest.Name;
            var message = string.Format("After : {0}.{1} took {2}ms", className, methodName, _sw.ElapsedMilliseconds);

            Trace.WriteLine(message);
        }
    }
}