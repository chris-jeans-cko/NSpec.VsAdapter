﻿using Autofac;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using NSpec.VsAdapter.Execution;
using NSpec.VsAdapter.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.TestAdapter
{
    [ExtensionUri(Constants.ExecutorUriString)]
    public class NSpecTestExecutor : ITestExecutor, IDisposable
    {
        // used by Visual Studio test infrastructure, by integration tests
        public NSpecTestExecutor()
        {
            var scope = DIContainer.Instance.BeginScope();

            disposable = scope;

            this.binaryTestExecutor = scope.Resolve<IBinaryTestExecutor>();
            this.progressRecorderFactory = scope.Resolve<IProgressRecorderFactory>();
            this.loggerFactory = scope.Resolve<ILoggerFactory>();
        }

        // used by unit tests
        public NSpecTestExecutor(
            IBinaryTestExecutor binaryTestExecutor,
            IProgressRecorderFactory progressRecorderFactory,
            ILoggerFactory loggerFactory)
        {
            this.binaryTestExecutor = binaryTestExecutor;
            this.progressRecorderFactory = progressRecorderFactory;
            this.loggerFactory = loggerFactory;

            disposable = Disposable.Empty;
        }

        public void Dispose()
        {
            disposable.Dispose();
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var outputLogger = loggerFactory.CreateOutput((IMessageLogger)frameworkHandle);

            outputLogger.Info("Execution by source paths started");

            using (var progressRecorder = progressRecorderFactory.Create((ITestExecutionRecorder)frameworkHandle))
            using (var crossDomainLogger = new CrossDomainLogger(outputLogger))
            {
                foreach (var binaryPath in sources)
                {
                    progressRecorder.BinaryPath = binaryPath;

                    binaryTestExecutor.Execute(binaryPath, progressRecorder, outputLogger, crossDomainLogger);
                }
            }

            outputLogger.Info("Execution by source paths finished");
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            var outputLogger = loggerFactory.CreateOutput(frameworkHandle);

            outputLogger.Info("Execution by TestCases started");

            var testCaseGroupsBySource = tests.GroupBy(t => t.Source);

            using (var progressRecorder = progressRecorderFactory.Create((ITestExecutionRecorder)frameworkHandle))
            using (var crossDomainLogger = new CrossDomainLogger(outputLogger))
            {
                foreach (var group in testCaseGroupsBySource)
                {
                    string binaryPath = group.Key;

                    var testCaseFullNames = group.Select(tc => tc.FullyQualifiedName);

                    progressRecorder.BinaryPath = binaryPath;

                    binaryTestExecutor.Execute(binaryPath, testCaseFullNames, progressRecorder, outputLogger, crossDomainLogger);
                }
            }

            outputLogger.Info("Execution by TestCases finished");
        }

        public void Cancel()
        {
            // TODO implement execution cancel

            throw new NotImplementedException();
        }

        readonly IBinaryTestExecutor binaryTestExecutor;
        readonly IProgressRecorderFactory progressRecorderFactory;
        readonly ILoggerFactory loggerFactory;
        readonly IDisposable disposable;
    }
}
