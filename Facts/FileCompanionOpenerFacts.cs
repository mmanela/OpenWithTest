using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace MattManela.OpenWithTest.Facts
{
    public class FileCompanionOpenerFacts
    {
        public class TestableFileCompanionOpener : FileCompanionOpener
        {
            public Mock<ILogger> MoqLogger { get; set; }
            public Mock<ISolutionIndexService> MoqIndexService { get; set; }
            public Mock<IVisualStudioCommands> MoqVisualStudioCommands { get; set; }
            public Mock<IFileCompanionFinder> MoqFileCompanionFinder { get; set; }

            private TestableFileCompanionOpener(
                Mock<ILogger> moqLogger,
                Mock<ISolutionIndexService> moqIndexService,
                Mock<IVisualStudioCommands> moqVisualStudioCommands,
                Mock<IFileCompanionFinder> moqFileCompanionFinder)
                : base(moqLogger.Object, moqIndexService.Object, moqVisualStudioCommands.Object, moqFileCompanionFinder.Object)
            {
                MoqLogger = moqLogger;
                MoqIndexService = moqIndexService;
                MoqVisualStudioCommands = moqVisualStudioCommands;
                MoqFileCompanionFinder = moqFileCompanionFinder;
            }

            public static TestableFileCompanionOpener Create()
            {
                var opener = new TestableFileCompanionOpener(new Mock<ILogger>(), new Mock<ISolutionIndexService>(), new Mock<IVisualStudioCommands>(), new Mock<IFileCompanionFinder>());
                return opener;
            }
        }

        public class OpenCompanion
        {

            [Fact]
            public void Will_open_nothing_when_file_is_opened_already()
            {
                var opener = TestableFileCompanionOpener.Create();
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\file.cs")).Returns(true);

                opener.OpenFileCompanion(@"c:\my\path\file.cs", new[] { "tests" });

                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\file.cs"), Times.Never());
                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\filetests.cs"), Times.Never());
            }

            [Fact]
            public void Will_open_nothing_when_no_file_companions_are_found()
            {
                var opener = TestableFileCompanionOpener.Create();
                var suffixes = new[] {"tests"};
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\file.cs")).Returns(false);
                opener.MoqFileCompanionFinder
                    .Setup(x => x.FindFileCompanions(@"c:\my\path\file.cs", suffixes))
                    .Returns(new List<string>());
              
                opener.OpenFileCompanion(@"c:\my\path\file.cs", suffixes);

                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\file.cs"), Times.Never());
                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\filetests.cs"), Times.Never());
            }

            [Fact]
            public void Will_open_nothing_when_any_file_companions_are_already_open()
            {
                var opener = TestableFileCompanionOpener.Create();
                var suffixes = new[] { "tests" };
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\file.cs")).Returns(false);
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\filetests.cs")).Returns(true);
                opener.MoqFileCompanionFinder
                    .Setup(x => x.FindFileCompanions(@"c:\my\path\file.cs", suffixes))
                    .Returns(new List<string> { @"c:\my\path\filetests.cs" });

                opener.OpenFileCompanion(@"c:\my\path\file.cs", suffixes);

                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\file.cs"), Times.Never());
                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\filetests.cs"), Times.Never());
            }

            [Fact]
            public void Will_open_file_companion_and_original_file()
            {
                var opener = TestableFileCompanionOpener.Create();
                var suffixes = new[] { "tests" };
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\file.cs")).Returns(false);
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\filetests.cs")).Returns(false);
                opener.MoqFileCompanionFinder
                    .Setup(x => x.FindFileCompanions(@"c:\my\path\file.cs", suffixes))
                    .Returns(new List<string> { @"c:\my\path\filetests.cs" });

                opener.OpenFileCompanion(@"c:\my\path\file.cs", suffixes);

                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\file.cs"));
                opener.MoqVisualStudioCommands.Verify(x => x.OpenDocument(@"c:\my\path\filetests.cs"));
            }

            [Fact]
            public void Will_log_exceptions_when_opening_files()
            {
                var opener = TestableFileCompanionOpener.Create();
                var suffixes = new[] { "tests" };
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\file.cs")).Returns(false);
                opener.MoqIndexService.Setup(x => x.IsFileOpen(@"c:\my\path\filetests.cs")).Returns(false);
                opener.MoqFileCompanionFinder
                    .Setup(x => x.FindFileCompanions(@"c:\my\path\file.cs", suffixes))
                    .Returns(new List<string> { @"c:\my\path\filetests.cs" });
                var exception = new Exception();
                opener.MoqVisualStudioCommands.Setup(x => x.OpenDocument(It.IsAny<string>())).Throws(exception);

                opener.OpenFileCompanion(@"c:\my\path\file.cs", suffixes);

                opener.MoqLogger.Verify(x => x.Log("Unable to open documents", "FileComanionOpener", exception));
            }

        }

    }
}