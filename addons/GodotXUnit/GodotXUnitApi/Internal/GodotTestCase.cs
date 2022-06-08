using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace GodotXUnitApi.Internal
{
    public class GodotTestCase : XunitTestCase
    {
        private IAttributeInfo attribute;
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
        public GodotTestCase() {}
        
        public GodotTestCase(IAttributeInfo attribute,
                             IMessageSink diagnosticMessageSink,
                             ITestFrameworkDiscoveryOptions discoveryOptions,
                             ITestMethod testMethod,
                             object[] testMethodArguments = null)
            : base(diagnosticMessageSink,
                   discoveryOptions.MethodDisplayOrDefault(),
                   discoveryOptions.MethodDisplayOptionsOrDefault(),
                   testMethod,
                   testMethodArguments)
        {
            this.attribute = attribute;
        }

        public override async Task<RunSummary> RunAsync(IMessageSink diagnosticMessageSink,
                                                        IMessageBus messageBus,
                                                        object[] constructorArguments,
                                                        ExceptionAggregator aggregator,
                                                        CancellationTokenSource cancellationTokenSource)
        {
            return await new GodotTestCaseRunner(attribute,
                                                 this,
                                                 DisplayName,
                                                 SkipReason,
                                                 constructorArguments, 
                                                 TestMethodArguments,
                                                 messageBus,
                                                 aggregator,
                                                 cancellationTokenSource)
                .RunAsync();
        }
    }

    public class GodotTestCaseRunner : XunitTestCaseRunner
    {
        private IAttributeInfo attribute;
        
        public GodotTestCaseRunner(IAttributeInfo attribute,
                                   IXunitTestCase testCase,
                                   string displayName,
                                   string skipReason,
                                   object[] constructorArguments,
                                   object[] testMethodArguments,
                                   IMessageBus messageBus,
                                   ExceptionAggregator aggregator,
                                   CancellationTokenSource cancellationTokenSource)
            : base(testCase,
                   displayName,
                   skipReason,
                   constructorArguments,
                   testMethodArguments,
                   messageBus,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
        }

        protected override XunitTestRunner CreateTestRunner(
            ITest test,
            IMessageBus messageBus,
            Type testClass,
            object[] constructorArguments,
            MethodInfo testMethod,
            object[] testMethodArguments,
            string skipReason,
            IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
            ExceptionAggregator aggregator,
            CancellationTokenSource cancellationTokenSource)
        {
            return new GodotTestRunner(attribute,
                                       test,
                                       messageBus, 
                                       testClass,
                                       constructorArguments,
                                       testMethod,
                                       testMethodArguments,
                                       skipReason,
                                       beforeAfterAttributes,
                                       new ExceptionAggregator(aggregator),
                                       cancellationTokenSource);
        }
    }

    public class GodotTestRunner : XunitTestRunner
    {
        private IAttributeInfo attribute;

        public GodotTestRunner(IAttributeInfo attribute,
                               ITest test,
                               IMessageBus messageBus,
                               Type testClass,
                               object[] constructorArguments,
                               MethodInfo testMethod,
                               object[] testMethodArguments,
                               string skipReason,
                               IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                               ExceptionAggregator aggregator,
                               CancellationTokenSource cancellationTokenSource)
            : base(test,
                   messageBus,
                   testClass,
                   constructorArguments,
                   testMethod,
                   testMethodArguments,
                   skipReason,
                   beforeAfterAttributes,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
        }
        
        protected override async Task<Tuple<Decimal, string>> InvokeTestAsync(ExceptionAggregator aggregator)
        {
            
            // override the ITestOutputHelper from XunitTestClassRunner
            TestOutputHelper helper = null; 
            for (int i = 0; i < ConstructorArguments.Length; i++)
            {
                if (ConstructorArguments[i] is ITestOutputHelper)
                {
                    helper = (TestOutputHelper) ConstructorArguments[i];
                    break;
                }
            }
            var output = new GodotTestOutputHelper(helper);
            output.Initialize(MessageBus, Test);
            var runTime = await InvokeTestMethodAsync(aggregator);
            return Tuple.Create(runTime, output.UnInitAndGetOutput());
        }

        protected override Task<Decimal> InvokeTestMethodAsync(ExceptionAggregator aggregator)
        {
            return new GodotTestInvoker(attribute,
                                        this.Test,
                                        this.MessageBus,
                                        this.TestClass,
                                        this.ConstructorArguments,
                                        this.TestMethod,
                                        this.TestMethodArguments,
                                        this.BeforeAfterAttributes,
                                        aggregator,
                                        this.CancellationTokenSource)
                .RunAsync();
        }
    }

    public class GodotTestInvoker : XunitTestInvoker
    {
        private IAttributeInfo attribute;
        
        private Node addingToTree;

        private bool loadEmptyScene;

        public GodotTestInvoker(IAttributeInfo attribute,
                                ITest test,
                                IMessageBus messageBus,
                                Type testClass,
                                object[] constructorArguments,
                                MethodInfo testMethod,
                                object[] testMethodArguments,
                                IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
                                ExceptionAggregator aggregator,
                                CancellationTokenSource cancellationTokenSource)
            : base(test,
                   messageBus, 
                   testClass, 
                   constructorArguments,
                   testMethod,
                   testMethodArguments,
                   beforeAfterAttributes,
                   aggregator,
                   cancellationTokenSource)
        {
            this.attribute = attribute;
        }

        protected override object CreateTestClass()
        {
            var check = base.CreateTestClass();
            if (check is Node node)
                addingToTree = node;
            
            return check;
        }

        protected override async Task BeforeTestMethodInvokedAsync()
        {
            var sceneCheck = attribute.GetNamedArgument<string>(nameof(GodotFactAttribute.Scene));
            if (!string.IsNullOrEmpty(sceneCheck))
            {
                // you must be in the process frame to 
                await GDU.OnProcessAwaiter;
                if (GDU.Instance.GetTree().ChangeScene(sceneCheck) != Error.Ok)
                {
                    Aggregator.Add(new Exception($"could not load scene: {sceneCheck}"));
                    return;
                }
                loadEmptyScene = true;
            
                // the scene should be loaded within two frames
                await GDU.OnIdleFrameAwaiter;
                await GDU.OnIdleFrameAwaiter;
                await GDU.OnProcessAwaiter;
            }
            
            if (addingToTree != null)
            {
                await GDU.OnProcessAwaiter;
                GDU.Instance.AddChild(addingToTree);
                await GDU.OnProcessAwaiter;
            }

            await base.BeforeTestMethodInvokedAsync();
        }

        protected override async Task<decimal> InvokeTestMethodAsync(object testClassInstance)
        {
            var sceneCheck = attribute.GetNamedArgument<GodotFactFrame>(nameof(GodotFactAttribute.Frame));
            switch (sceneCheck)
            {
                case GodotFactFrame.Default:
                    break;
                case GodotFactFrame.Process:
                    await GDU.OnProcessAwaiter;
                    break;
                case GodotFactFrame.PhysicsProcess:
                    await GDU.OnPhysicsProcessAwaiter;
                    break;
                default:
                    Aggregator.Add(new Exception($"unknown GodotFactFrame: {sceneCheck.ToString()}"));
                    throw new ArgumentOutOfRangeException();
            }
            return await base.InvokeTestMethodAsync(testClassInstance);
        }

        protected override async Task AfterTestMethodInvokedAsync()
        {
            await base.AfterTestMethodInvokedAsync();

            if (addingToTree != null)
            {
                await GDU.OnProcessAwaiter;
                GDU.Instance.RemoveChild(addingToTree);
                await GDU.OnProcessAwaiter;
            }

            if (loadEmptyScene)
            {
                // change scenes again and wait for godot to catch up
                GDU.Instance.GetTree().ChangeScene(Consts.EMPTY_SCENE_PATH);
                await GDU.OnIdleFrameAwaiter;
                await GDU.OnIdleFrameAwaiter;
                await GDU.OnProcessAwaiter;
            }
        }
    }
}