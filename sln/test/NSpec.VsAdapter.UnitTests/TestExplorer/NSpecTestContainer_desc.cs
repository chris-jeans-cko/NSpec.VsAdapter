﻿using AutofacContrib.NSubstitute;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestWindow.Extensibility;
using NSpec.VsAdapter.Common;
using NSpec.VsAdapter.TestExplorer;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.UnitTests.TestExplorer
{
    [TestFixture]
    [Category("NSpecTestContainer")]
    public abstract class NSpecTestContainer_desc_base
    {
        protected NSpecTestContainer container;

        protected AutoSubstitute autoSubstitute;
        protected ITestContainerDiscoverer containerDiscoverer;
        protected string sourcePath = "some/dummy/source";
        protected IEnumerable<Guid> debugEngines;
        protected IFileService fileService;

        protected DateTime someTime;

        [SetUp]
        public virtual void before_each()
        {
            autoSubstitute = new AutoSubstitute();

            containerDiscoverer = autoSubstitute.Resolve<ITestContainerDiscoverer>();

            debugEngines = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

            fileService = autoSubstitute.Resolve<IFileService>();

            someTime = new DateTime(2015, 10, 21);

            fileService.Exists(Arg.Any<string>()).Returns(true);
            fileService.LastModified(Arg.Any<string>()).Returns(someTime);

            container = new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, fileService);
        }

        [TearDown]
        public virtual void after_each()
        {
            autoSubstitute.Dispose();
        }
    }

    public class NSpecTestContainer_when_creating : NSpecTestContainer_desc_base
    {
        [Test]
        public void it_should_return_container_discoverer()
        {
            container.Discoverer.Should().Be(containerDiscoverer);
        }

        [Test]
        public void it_should_return_source()
        {
            container.Source.Should().Be(sourcePath);
        }

        [Test]
        public void it_should_return_debug_engines()
        {
            container.DebugEngines.Should().Equal(debugEngines);
        }

        [Test]
        public void it_should_not_target_any_framework_version()
        {
            container.TargetFramework.Should().Be(FrameworkVersion.None);
        }

        [Test]
        public void it_should_target_any_cpu_platform()
        {
            container.TargetPlatform.Should().Be(Architecture.AnyCPU);
        }

        [Test]
        public void it_should_not_be_a_win8_metro_test_container()
        {
            container.IsAppContainerTestContainer.Should().BeFalse();

            container.DeployAppContainer().Should().BeNull();
        }

        [Test]
        public void it_should_throw_with_null_container_discoverer()
        {
            Assert.That(() =>
            {
                new NSpecTestContainer(null, sourcePath, debugEngines, fileService);

            }, Throws.Exception);
        }

        [Test]
        public void it_should_throw_with_null_source()
        {
            Assert.That(() =>
            {
                new NSpecTestContainer(containerDiscoverer, null, debugEngines, fileService);

            }, Throws.Exception);
        }

        [Test]
        public void it_should_throw_with_empty_source()
        {
            Assert.That(() =>
            {
                new NSpecTestContainer(containerDiscoverer, String.Empty, debugEngines, fileService);

            }, Throws.Exception);
        }

        [Test]
        public void it_should_throw_with_null_engines()
        {
            Assert.That(() =>
            {
                new NSpecTestContainer(containerDiscoverer, sourcePath, null, fileService);

            }, Throws.Exception);
        }

        [Test]
        public void it_should_throw_with_null_fileservice()
        {
            Assert.That(() =>
            {
                new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, null);

            }, Throws.Exception);
        }
    }

    public class NSpecTestContainer_when_comparing : NSpecTestContainer_desc_base
    {
        NSpecTestContainer other;
        string otherSourcePath;

        public override void before_each()
        {
            base.before_each();

            otherSourcePath = sourcePath + "/whatever";
        }

        [Test]
        public void it_should_be_smaller_when_other_is_null()
        {
            other = null;

            container.CompareTo(other).Should().BeNegative();
        }

        [Test]
        public void it_should_be_smaller_when_source_is_not_found()
        {
            fileService.Exists(sourcePath).Returns(false);

            // recreate
            container = new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, fileService);

            other = new NSpecTestContainer(containerDiscoverer, otherSourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().BeNegative();
        }

        [Test]
        public void it_should_be_smaller_when_other_source_is_not_found()
        {
            fileService.Exists(otherSourcePath).Returns(false);

            other = new NSpecTestContainer(containerDiscoverer, otherSourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().BeNegative();
        }

        [Test]
        public void it_should_be_smaller_when_sources_are_different()
        {
            other = new NSpecTestContainer(containerDiscoverer, otherSourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().BeNegative();
        }

        [Test]
        public void it_should_be_smaller_when_sources_are_equal_and_timestamp_is_before_other()
        {
            var otherTimestamp = someTime.Add(TimeSpan.FromHours(2));

            fileService.LastModified(sourcePath).Returns(otherTimestamp);

            other = new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().BeNegative();
        }

        [Test]
        public void it_should_be_larger_when_sources_are_equal_and_timestamp_is_after_other()
        {
            var otherTimestamp = someTime.Subtract(TimeSpan.FromHours(2));

            fileService.LastModified(sourcePath).Returns(otherTimestamp);

            other = new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().BePositive();
        }

        [Test]
        public void it_should_match_when_when_sources_are_equal_and_timestamps_are_equal()
        {
            other = new NSpecTestContainer(containerDiscoverer, sourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().Be(0);
        }

        [Test]
        public void it_should_match_when_sources_have_different_case_and_timestamps_are_equal()
        {
            otherSourcePath = FlipCase(sourcePath);

            other = new NSpecTestContainer(containerDiscoverer, otherSourcePath, debugEngines, fileService);

            container.CompareTo(other).Should().Be(0);
        }

        private static String FlipCase(String orig)
        {
            Func<char, String> selector =
                c => (char.IsUpper(c) ? char.ToLower(c) : char.ToUpper(c)).ToString();

            return orig.Select(selector).Aggregate(String.Concat);
        }
    }

    [TestFixture]
    [Category("Container")]
    public class NSpecTestContainer_when_cloning : NSpecTestContainer_desc_base
    {
        ITestContainer snapshot;

        public override void before_each()
        {
            base.before_each();

            snapshot = container.Snapshot();
        }

        [Test]
        public void it_should_return_same_properties()
        {
            snapshot.Discoverer.Should().Be(container.Discoverer);

            snapshot.Source.Should().Be(container.Source);

            snapshot.DebugEngines.Should().Equal(container.DebugEngines);

            snapshot.TargetFramework.Should().Be(container.TargetFramework);

            snapshot.TargetPlatform.Should().Be(container.TargetPlatform);

            snapshot.IsAppContainerTestContainer.Should().Be(container.IsAppContainerTestContainer);

            snapshot.DeployAppContainer().Should().Be(container.DeployAppContainer());
        }

        [Test]
        public void it_should_match_snapshot()
        {
            container.CompareTo(snapshot).Should().Be(0);
        }
    }
}
