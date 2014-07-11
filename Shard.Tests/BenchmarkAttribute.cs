using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace Shard.Tests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BenchmarkAttribute : FactAttribute
    {
        private readonly int _runCount;

        public BenchmarkAttribute(int runCount = 100)
        {
            _runCount = runCount;
        }

        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            return from testCase in base.EnumerateTestCommands(method)
                from run in Enumerable.Repeat(testCase, _runCount)
                select new TimedCommand(run);
        }
    }
}