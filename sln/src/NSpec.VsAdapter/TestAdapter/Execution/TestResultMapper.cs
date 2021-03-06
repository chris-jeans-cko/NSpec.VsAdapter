﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NSpec.VsAdapter.Core.Execution;

namespace NSpec.VsAdapter.TestAdapter.Execution
{
    public class TestResultMapper : ITestResultMapper
    {
        public TestResult FromExecutedExample(ExecutedExample executedExample, string binaryPath)
        {
            var testCase = new TestCase(executedExample.FullName, Constants.ExecutorUri, binaryPath);

            var testResult = new TestResult(testCase);

            if (executedExample.Pending)
            {
                testResult.Outcome = TestOutcome.Skipped;
            }
            else if (executedExample.Failed)
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = executedExample.ExceptionMessage;
                testResult.ErrorStackTrace = executedExample.ExceptionStackTrace;
            }
            else
            {
                testResult.Outcome = TestOutcome.Passed;
            }

            return testResult;
        }
    }
}
