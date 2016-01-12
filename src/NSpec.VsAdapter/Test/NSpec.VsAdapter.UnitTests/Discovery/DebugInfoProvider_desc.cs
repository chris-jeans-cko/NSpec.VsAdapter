﻿using AutofacContrib.NSubstitute;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NSpec.VsAdapter.Discovery;
using NSpec.VsAdapter.UnitTests.Discovery.SampleSpecs;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NSpec.VsAdapter.UnitTests.Discovery
{
    [TestFixture]
    [Category("DebugInfoProvider")]
    public abstract class DebugInfoProvider_desc_base
    {
        protected DebugInfoProvider provider;

        protected AutoSubstitute autoSubstitute;
        protected ISerializableLogger logger;

        [SetUp]
        public virtual void before_each()
        {
            autoSubstitute = new AutoSubstitute();

            logger = autoSubstitute.Resolve<ISerializableLogger>();
        }

        [TearDown]
        public virtual void after_each()
        {
            autoSubstitute.Dispose();
        }
    }

    public class DebugInfoProvider_when_binary_exists : DebugInfoProvider_desc_base
    {
        public override void before_each()
        {
            base.before_each();

            string binaryPath = SpecSampleFileInfo.ThisBinaryFilePath;

            provider = new DebugInfoProvider(binaryPath, logger);
        }

        [Test]
        public void it_should_return_navigation_data()
        {
            SampleTestCaseDebugInfo.ByClassMethodName.Keys.Do(declaringClassName =>
                {
                    var methodInfos = SampleTestCaseDebugInfo.ByClassMethodName[declaringClassName];

                    methodInfos.Keys.Do(methodName =>
                        {
                            DiaNavigationData expected = methodInfos[methodName];

                            DiaNavigationData actual = provider.GetNavigationData(declaringClassName, methodName);

                            actual.ShouldBeEquivalentTo(expected, 
                                "ClassName: {0}, MethodName: {1}", declaringClassName, methodName);
                        });
                });
        }
    }

    public class DebugInfoProvider_when_binary_not_found : DebugInfoProvider_desc_base
    {
        public override void before_each()
        {
            base.before_each();

            string wrongBinaryPath = @".\some\wrong\path\to\library.dll";

            provider = new DebugInfoProvider(wrongBinaryPath, logger);
        }

        [Test]
        public void it_should_return_no_navigation_data()
        {
            DiaNavigationData expected = new DiaNavigationData(String.Empty, 0, 0);

            SampleTestCaseDebugInfo.ByClassMethodName.Keys.Do(declaringClassName =>
            {
                var methodInfos = SampleTestCaseDebugInfo.ByClassMethodName[declaringClassName];

                methodInfos.Keys.Do(methodName =>
                {
                    DiaNavigationData actual = provider.GetNavigationData(declaringClassName, methodName);

                    actual.ShouldBeEquivalentTo(expected,
                        "ClassName: {0}, MethodName: {1}", declaringClassName, methodName);
                });
            });
        }
    }
}
