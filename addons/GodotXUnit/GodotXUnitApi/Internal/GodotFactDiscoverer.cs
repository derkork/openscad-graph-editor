using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public class GodotFactDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink diagnosticMessageSink;

        public GodotFactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions options,
                                                    ITestMethod method,
                                                    IAttributeInfo attribute)
        {
            yield return new GodotTestCase(attribute, diagnosticMessageSink, options, method);
        }
    }
}