using System;
using System.Collections.Concurrent;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Xunit.Runners;
using Path = System.IO.Path;
using Directory = System.IO.Directory;

namespace GodotXUnitApi.Internal
{
    public abstract class GodotXUnitRunnerBase : Node2D
    {
        protected virtual Assembly GetTargetAssembly(GodotXUnitSummary summary)
        {
            // check if we even have a project assembly set
            if (!ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY))
                return Assembly.GetExecutingAssembly();

            // get the project and if its the default (the godot project), return executing
            var targetProject = ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY).ToString();
            if (string.IsNullOrEmpty(targetProject) || targetProject.Equals(ProjectListing.GetDefaultProject()))
                return Assembly.GetExecutingAssembly();

            // if its a custom project target, attempt to just load the assembly directly
            if (targetProject.Equals(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM_FLAG))
            {
                var customDll = ProjectSettings.HasSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM)
                    ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_ASSEMBLY_CUSTOM).ToString()
                    : "";
                if (string.IsNullOrEmpty(customDll))
                {
                    summary.AddDiagnostic("no custom dll assembly configured.");
                    GD.PrintErr("no custom dll assembly configured.");
                    return Assembly.GetExecutingAssembly();
                }

                summary.AddDiagnostic($"attempting to load custom dll at: {customDll}");
                return AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(customDll));
            }

            // find the project in the project list. if its not there, print error and leave
            var projectList = ProjectListing.GetProjectInfo();
            if (!projectList.ContainsKey(targetProject))
            {
                GD.PrintErr(
                    $"unable to find project {targetProject}. expected values: {string.Join(", ", projectList.Keys)}");
                return Assembly.GetExecutingAssembly();
            }

            // finally, attempt to load project..
            var currentDir = Directory.GetCurrentDirectory();
            var targetAssembly = Path.Combine(currentDir, $".mono/build/bin/Debug/{targetProject}.dll");
            var name = AssemblyName.GetAssemblyName(targetAssembly);
            return AppDomain.CurrentDomain.Load(name);
        }

        protected virtual string GetTargetClass(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_CLASS)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_CLASS)?.ToString()
                : null;
        }

        protected virtual string GetTargetMethod(GodotXUnitSummary summary)
        {
            return ProjectSettings.HasSetting(Consts.SETTING_TARGET_METHOD)
                ? ProjectSettings.GetSetting(Consts.SETTING_TARGET_METHOD)?.ToString()
                : null;
        }

        private ConcurrentQueue<Action<Node2D>> drawRequests = new ConcurrentQueue<Action<Node2D>>();

        [Signal]
        public delegate void OnProcess();

        [Signal]
        public delegate void OnPhysicsProcess();

        [Signal]
        public delegate void OnDrawRequestDone();

        public void RequestDraw(Action<Node2D> request)
        {
            drawRequests.Enqueue(request);
        }

        private AssemblyRunner runner;

        private GodotXUnitSummary summary;

        private MessageSender messages;

        public override void _Ready()
        {
            GDU.Instance = this;
            GD.Print($"running tests in tree at: {GetPath()}");
            WorkFiles.CleanWorkDir();
            summary = new GodotXUnitSummary();
            messages = new MessageSender();
            CreateRunner();
            if (runner == null)
            {
                messages.SendMessage(summary, "summary");
                WriteSummary(summary);
                GetTree().Quit(1);
                return;
            }
            runner.OnDiagnosticMessage = message =>
            {
                GD.PrintErr($"OnDiagnosticMessage: {message.Message}");
                summary.AddDiagnostic(message.Message);
            };
            runner.OnDiscoveryComplete = message =>
            {
                summary.testsDiscovered = message.TestCasesDiscovered;
                summary.testsExpectedToRun = message.TestCasesToRun;
                GD.Print($"discovery finished: found {message.TestCasesDiscovered}," +
                         $" running {message.TestCasesToRun}");
            };
            runner.OnErrorMessage = message =>
            {
                GD.PrintErr($"OnErrorMessage ({message.MesssageType}) {message.ExceptionType}: " +
                            $"{message.ExceptionMessage}\n{message.ExceptionStackTrace}");
                summary.diagnostics.Add(new GodotXUnitOtherDiagnostic
                {
                    message = message.ExceptionMessage,
                    exceptionType = message.ExceptionType,
                    exceptionStackTrace = message.ExceptionStackTrace
                });
            };
            runner.OnTestStarting = message =>
            {
                messages.SendMessage(new GodotXUnitTestStart()
                {
                    testCaseClass = message.TypeName,
                    testCaseName = message.MethodName
                }, "start");
            };
            runner.OnTestFailed = message =>
            {
                messages.SendMessage(summary.AddFailed(message), "failed");
                GD.Print($"  > OnTestFailed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestPassed = message =>
            {
                messages.SendMessage(summary.AddPassed(message), "passed");
                GD.Print($"  > OnTestPassed: {message.TestDisplayName} in {message.ExecutionTime}");
            };
            runner.OnTestSkipped = message =>
            {
                messages.SendMessage(summary.AddSkipped(message), "skipped");
                GD.Print($"  > OnTestSkipped: {message.TestDisplayName}");
            };
            runner.OnExecutionComplete = message =>
            {
                messages.SendMessage(summary, "summary");
                WriteSummary(summary);
                GD.Print($"tests completed ({message.ExecutionTime}): {summary.completed}");
                GetTree().Quit(Mathf.Clamp(summary.failed.Count, 0, 20));
            };

            var targetMethod = GetTargetMethod(summary);
            if (!string.IsNullOrEmpty(targetMethod))
            {
                GD.Print($"targeting method for discovery: {targetMethod}");
                runner.TestCaseFilter = test => targetMethod.Equals(test.TestMethod.Method.Name);
            }
            
            // if its an empty string, then we need to set it to null because the runner only checks for null
            var targetClass = GetTargetClass(summary);
            if (string.IsNullOrEmpty(targetClass))
                targetClass = null;
            else
            {
                GD.Print($"targeting class for discovery: {targetClass}");
            }
            runner.Start(targetClass, null, null, null, null, false, null, null);
        }

        private void CreateRunner()
        {
            try
            {
                var check = GetTargetAssembly(summary);
                if (check == null)
                {
                    GD.PrintErr("no assembly returned for tests");
                    summary.AddDiagnostic(new Exception("no assembly returned for tests"));
                    return;
                }
                runner = AssemblyRunner.WithoutAppDomain(check.Location);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"error while attempting to get test assembly: {ex}");
                summary.AddDiagnostic("error while attempting to get test assembly");
                summary.AddDiagnostic(ex);
            }
        }

        public override void _ExitTree()
        {
            GDU.Instance = null;
            runner?.Dispose();
            runner = null;
        }

        private void WriteSummary(GodotXUnitSummary testSummary)
        {
            var location = ProjectSettings.HasSetting(Consts.SETTING_RESULTS_SUMMARY)
                ? ProjectSettings.GetSetting(Consts.SETTING_RESULTS_SUMMARY).ToString()
                : Consts.SETTING_RESULTS_SUMMARY_DEF;
            var file = new Godot.File();
            var result = file.Open(location, Godot.File.ModeFlags.Write);
            if (result == Error.Ok)
                file.StoreString(JsonConvert.SerializeObject(testSummary, Formatting.Indented, WorkFiles.jsonSettings));
            else
                GD.Print($"error returned for writing message at {location}: {result}");
            file.Close();
        }

        public override void _Process(float delta)
        {
            EmitSignal(nameof(OnProcess));
            Update();
        }

        public override void _PhysicsProcess(float delta)
        {
            EmitSignal(nameof(OnPhysicsProcess));
        }

        public override void _Draw()
        {
            while (drawRequests.TryDequeue(out var request))
                request(this);
            EmitSignal(nameof(OnDrawRequestDone));
        }
    }
}