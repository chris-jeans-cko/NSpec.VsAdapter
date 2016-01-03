﻿using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.ProjectObservation
{
    public class ProjectNotifier : IProjectNotifier, IDisposable
    {
        public ProjectNotifier(ISolutionNotifier solutionNotifier, IProjectEnumerator projectEnumerator)
        {
            var hotProjectStream = solutionNotifier.SolutionOpenedStream.Select(solutionInfo =>
                {
                    var projectInfos = projectEnumerator.GetLoadedProjects(solutionInfo);

                    var solutionChangeStream = solutionNotifier.ProjectAddedStream
                        .Merge(solutionNotifier.ProjectRemovingtream)
                        .TakeUntil(solutionNotifier.SolutionClosingStream);

                    var emptyProjectInfos = new ProjectInfo[0];

                    var solutionClosedStream = solutionNotifier.SolutionClosingStream
                        .Select(_ => emptyProjectInfos);

                    var projectChangeStream = solutionChangeStream
                        .Select(_ => projectEnumerator.GetLoadedProjects(solutionInfo))
                        .Merge(solutionClosedStream)
                        .StartWith(projectInfos);

                    return projectChangeStream;
                })
                .Switch()
                .Replay(1);

            hotProjectStream.Connect().DisposeWith(disposables);

            ProjectStream = hotProjectStream;
        }

        public IObservable<IEnumerable<ProjectInfo>> ProjectStream { get; private set; }

        public void Dispose()
        {
            disposables.Dispose();
        }

        readonly CompositeDisposable disposables = new CompositeDisposable();
    }
}
