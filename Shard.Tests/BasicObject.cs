using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Shard.Tests
{
    class BasicObject
    {
        public string Id { get; set; }
        public string Value { get; set; }
    }

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

    class TestCommandWrapper : ITestCommand
    {
        private readonly ITestCommand _command;

        public TestCommandWrapper(ITestCommand command)
        {
            _command = command;
        }

        public MethodResult Execute(object testClass)
        {
            return _command.Execute(testClass);
        }

        public XmlNode ToStartXml()
        {
            return _command.ToStartXml();
        }

        public string DisplayName
        {
            get
            {
                return _command.DisplayName;
            }
        }

        public bool ShouldCreateInstance
        {
            get
            {
                return _command.ShouldCreateInstance;
            }
        }

        public int Timeout
        {
            get
            {
                return _command.Timeout;
            }
        }
    }

    public class TraceAttribute : BeforeAfterTestAttribute
    {
        private Stopwatch _sw;

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