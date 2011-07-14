using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace MattManela.OpenWithTest.Facts
{
    public class FileCompanionFinderFacts
    {
        public class TestableFileCompanionFinder : FileCompanionFinder
        {
            public Mock<ILogger> MoqLogger { get; set; }
            public Mock<ISolutionIndexService> MoqIndexService { get; set; }

            private TestableFileCompanionFinder(Mock<ILogger> moqLogger, Mock<ISolutionIndexService> moqIndexService)
                : base(moqLogger.Object, moqIndexService.Object)
            {
                MoqLogger = moqLogger;
                MoqIndexService = moqIndexService;
            }

            public static TestableFileCompanionFinder Create()
            {
                var finder = new TestableFileCompanionFinder(new Mock<ILogger>(), new Mock<ISolutionIndexService>());
                finder.MoqIndexService.Setup(x => x.GetPathsForFileName(It.IsAny<string>())).Returns(new List<string>());
                return finder;
            }
        }


        public class FindFileCompanions
        {
            [Fact]
            public void Will_retuen_one_test_file_when_only_one_matches()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filetests.cs"))
                    .Returns(new List<string> { @"c:\my\path\filetests.cs" });

                var companions = finder.FindFileCompanions(@"c:\my\path\file.cs", new[] { "tests" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\filetests.cs", companions.First());
            }

            [Fact]
            public void Will_return_test_file_for_first_suffix_that_returns_matches()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filetests.cs"))
                    .Returns(new List<string> { });
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filefacts.cs"))
                    .Returns(new List<string> { @"c:\my\path\filefacts.cs" });

                var companions = finder.FindFileCompanions(@"c:\my\path\file.cs", new[] { "tests", "facts" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\filefacts.cs", companions.First());
            }

            [Fact]
            public void Will_return_empty_collection_if_no_test_files_found()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filetests.cs"))
                    .Returns(new List<string> { });

                var companions = finder.FindFileCompanions(@"c:\my\path\file.cs", new[] { "tests" });

                Assert.Empty(companions);
            }

            [Fact]
            public void Will_retuen_best_matching_test_file()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filetests.cs"))
                    .Returns(new List<string> { @"c:\my\other\filetests.cs", @"c:\my\path\filetests.cs", });

                var companions = finder.FindFileCompanions(@"c:\my\path\file.cs", new[] { "tests" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\filetests.cs", companions.First());
            }

            [Fact]
            public void Will_retuen_multiple_best_matching_test_files_if_they_are_equally_good()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"filetests.cs"))
                    .Returns(new List<string> { @"c:\my\otherpath\filetests.cs", @"c:\my\realpath\filetests.cs", });

                var companions = finder.FindFileCompanions(@"c:\my\path\file.cs", new[] { "tests" });

                Assert.Equal(2, companions.Count());
                Assert.True(companions.Any(x => x.Equals(@"c:\my\otherpath\filetests.cs")));
                Assert.True(companions.Any(x => x.Equals(@"c:\my\realpath\filetests.cs")));
            }

            [Fact]
            public void Will_return_one_implementation_file_when_only_one_matches()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"file.cs"))
                    .Returns(new List<string> { @"c:\my\path\file.cs" });

                var companions = finder.FindFileCompanions(@"c:\my\path\filetests.cs", new[] { "tests" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\file.cs", companions.First());
            }

            [Fact]
            public void Will_return_implementation_file_when_second_suffix_matches()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"file.cs"))
                    .Returns(new List<string> { @"c:\my\path\file.cs" });

                var companions = finder.FindFileCompanions(@"c:\my\path\filefacts.cs", new[] { "tests", "facts" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\file.cs", companions.First());
            }

            [Fact]
            public void Will_return_empty_collection_if_no_implementation_files_found()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"file.cs"))
                    .Returns(new List<string> { });

                var companions = finder.FindFileCompanions(@"c:\my\path\filetests.cs", new[] { "tests" });

                Assert.Empty(companions);
            }

            [Fact]
            public void Will_retuen_best_matching_implementation_file()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"file.cs"))
                    .Returns(new List<string> { @"c:\my\other\file.cs", @"c:\my\path\file.cs", });

                var companions = finder.FindFileCompanions(@"c:\my\path\filetests.cs", new[] { "tests" });

                Assert.Single(companions);
                Assert.Equal(@"c:\my\path\file.cs", companions.First());
            }

            [Fact]
            public void Will_retuen_multiple_best_matching_implementation_files_if_they_are_equally_good()
            {
                var finder = TestableFileCompanionFinder.Create();
                finder.MoqIndexService
                    .Setup(x => x.GetPathsForFileName(@"file.cs"))
                    .Returns(new List<string> { @"c:\my\otherpath\file.cs", @"c:\my\realpath\file.cs", });

                var companions = finder.FindFileCompanions(@"c:\my\path\filetests.cs", new[] { "tests" });

                Assert.Equal(2, companions.Count());
                Assert.True(companions.Any(x => x.Equals(@"c:\my\otherpath\file.cs")));
                Assert.True(companions.Any(x => x.Equals(@"c:\my\realpath\file.cs")));
            }
        }
    }
}