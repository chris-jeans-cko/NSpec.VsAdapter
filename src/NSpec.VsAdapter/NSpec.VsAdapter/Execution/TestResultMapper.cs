﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NSpec.Domain;
using NSpec.VsAdapter.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.Execution
{
    public class TestResultMapper : ITestResultMapper
    {
        public TestResult FromExample(ExampleBase example)
        {
            // TODO obtain somehow the current binary path

            string dummyBinaryPath = @".\whatever\dummy-library.dll";

            var testCase = new TestCase(example.FullName(), Constants.ExecutorUri, dummyBinaryPath);

            var testResult = new TestResult(testCase);

            if (example.Pending)
            {
                testResult.Outcome = TestOutcome.Skipped;
            }
            else if (example.Failed())
            {
                testResult.Outcome = TestOutcome.Failed;
                testResult.ErrorMessage = example.Exception.Message;
                testResult.ErrorStackTrace = example.Exception.StackTrace;
            }
            else
            {
                testResult.Outcome = TestOutcome.Passed;
            }

            return testResult;
        }
    }
}
