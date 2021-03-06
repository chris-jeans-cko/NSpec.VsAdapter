﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using NSpec.VsAdapter.TestAdapter.Execution;

namespace NSpec.VsAdapter.Core.Execution
{
    public class ProgressRecorderFactory : IProgressRecorderFactory
    {
        public ProgressRecorderFactory(ITestResultMapper testResultMapper)
        {
            this.testResultMapper = testResultMapper;
        }

        public IProgressRecorder Create(ITestExecutionRecorder testExecutionRecorder)
        {
            return new ProgressRecorder(testExecutionRecorder, testResultMapper);
        }

        readonly ITestResultMapper testResultMapper;
    }
}
