// © Microsoft Corporation. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Xunit;

namespace Analyzer.Test
{
    public class UsingUnsealedNonPublicClassTests
    {
        [Fact]
        public async Task Analyzer()
        {
            const string source = @"
                /*0+*/internal class Test
                {
                }/*-0*/

                /*1+*/internal class Ext2 : UnsealedBase
                {
                }/*-1*/

                /*2+*/public class Public
                {
                    internal Public()
                    {
                    }
                }/*-2*/

                public class Public2
                {
                    public Public2()
                    {
                    }
                }

                internal class UnsealedBase
                {
                }

                internal sealed class Ext : UnsealedBase
                {
                }

                internal static class Static
                {
                }

                internal abstract class Abstract
                {
                }

                namespace Foo
                {
                    internal sealed class C1
                    {
                        /*3+*/private class C2
                        {
                        }/*-3*/
                    }

                    namespace Bar
                    {
                        public class C3
                        {
                            public class C4
                            {
                            }
                        }
                    }
                }
            ";

            var d = await RoslynTestUtils.RunAnalyzer(
                new UsingUnsealedNonPublicClassAnalyzer(),
                null,
                new[] { source }).ConfigureAwait(false);

            Assert.Equal(4, d.Count);
            source.AssertDiagnostic(0, DiagDescriptors.UsingUnsealedNonPublicClass, d[0]);
            source.AssertDiagnostic(1, DiagDescriptors.UsingUnsealedNonPublicClass, d[1]);
            source.AssertDiagnostic(2, DiagDescriptors.UsingUnsealedNonPublicClass, d[2]);
            source.AssertDiagnostic(3, DiagDescriptors.UsingUnsealedNonPublicClass, d[3]);
        }

        [Fact]
        public async Task Nested()
        {
            const string source = @"
                sealed class Test
                {
                    /*0+*/public class Nested
                    {
                    }/*-0*/
                }
            ";

            var diags = await RoslynTestUtils.RunAnalyzer(
                new UsingUnsealedNonPublicClassAnalyzer(),
                null,
                new[] { source }).ConfigureAwait(false);

            Assert.Single(diags);
            source.AssertDiagnostic(0, DiagDescriptors.UsingUnsealedNonPublicClass, diags[0]);
        }

        [Fact]
        public async Task Fixer()
        {
            const string source = @"
internal class Test
{
}
            ";

            const string expected = @"
internal sealed class Test
{
}
            ";

            var l = await RoslynTestUtils.RunAnalyzerAndFixer(
                new UsingUnsealedNonPublicClassAnalyzer(),
                new UsingUnsealedNonPublicClassFixer(),
                null,
                new[] { source }).ConfigureAwait(false);

            var actual = l[0];

            Assert.Equal(expected.Replace("\r\n", "\n", StringComparison.Ordinal), actual);
        }

        [Fact]
        public void ArgCheck()
        {
            var a = new UsingUnsealedNonPublicClassAnalyzer();
            Assert.Throws<ArgumentNullException>(() => a.Initialize(null!));
        }

        [Fact]
        public void UtilityMethods()
        {
            var f = new UsingUnsealedNonPublicClassFixer();
            Assert.Single(f.FixableDiagnosticIds);
            Assert.Equal(DiagDescriptors.UsingUnsealedNonPublicClass.Id, f.FixableDiagnosticIds[0]);
            Assert.Equal(WellKnownFixAllProviders.BatchFixer, f.GetFixAllProvider());
        }
    }
}
